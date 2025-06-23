using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Noesis;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace NoesisApp
{
    public class RenderContextD3D12 : RenderContext
    {
        public RenderContextD3D12()
        {
        }

        public override RenderDevice Device
        {
            get { return _device; }
        }

        private IntPtr _display;
        private IntPtr _window;

        private SharpDX.DXGI.SwapChainFlags _swapChainFlags = SharpDX.DXGI.SwapChainFlags.None;
        private SharpDX.DXGI.PresentFlags _presentFlags = SharpDX.DXGI.PresentFlags.None;
        private SharpDX.DXGI.SwapChain3 _swapChain;
        private SharpDX.DXGI.Format _format = SharpDX.DXGI.Format.Unknown;
        private SharpDX.DXGI.SampleDescription _sampleDesc;
        private int _syncInterval = 0;
        private int _frameIndex = 0;

        private RenderDeviceD3D12 _device;
        private SharpDX.Direct3D12.Device _dev;
        private SharpDX.Direct3D12.CommandQueue _queue;
        private SharpDX.Direct3D12.DescriptorHeap _heapRTV;
        private SharpDX.Direct3D12.DescriptorHeap _heapDSV;
        private int _sizeRTV = 0;

        private const int FrameCount = 2;
        private SharpDX.Direct3D12.Viewport[] _viewport = new SharpDX.Direct3D12.Viewport[1];
        private SharpDX.Mathematics.Interop.RawRectangle _scissorRect;
        private SharpDX.Direct3D12.Resource _stencilBuffer;
        private SharpDX.Direct3D12.Resource[] _backBuffers = new SharpDX.Direct3D12.Resource[FrameCount];
        private SharpDX.Direct3D12.Resource[] _backBuffersAA = new SharpDX.Direct3D12.Resource[FrameCount];
        private SharpDX.Direct3D12.CommandAllocator[] _commandAllocators = new SharpDX.Direct3D12.CommandAllocator[FrameCount];
        private SharpDX.Direct3D12.GraphicsCommandList _commands;
        private SharpDX.Direct3D12.Fence _fence;
        private AutoResetEvent _fenceEvent;
        long _fenceValue = 0;
        long[] _frameFenceValues = new long[FrameCount];

        private ImageCapture _imageCapture;

        public override void Init(IntPtr display, IntPtr window, uint samples, bool vsync, bool sRGB)
        {
            _display = display;
            _window = window;

            System.Console.WriteLine("Creating D3D12 render context");

            CreateDevice();
            CreateCommandQueue();
            CreateSwapChain(window, vsync);

            _format = sRGB ? SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb : SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            _sampleDesc = GetSampleDescription(samples);
            samples = (uint)_sampleDesc.Count;

            CreateHeaps();
            CreateBuffers();
            CreateCommandList();

            _device = new RenderDeviceD3D12(_dev.NativePointer, _fence.NativePointer, (int)_format, (int)SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                _sampleDesc.Count, sRGB);
        }

        private void CreateDevice()
        {
            using (SharpDX.DXGI.Factory4 factory = new Factory4())
            {
                _dev = new SharpDX.Direct3D12.Device(factory.Adapters[0], FeatureLevel.Level_11_0);
            }

            int[] levels_ =
            {
                (int)FeatureLevel.Level_12_1,
                (int)FeatureLevel.Level_12_0,
                (int)FeatureLevel.Level_11_1,
                (int)FeatureLevel.Level_11_0
            };

            GCHandle pinnedArray = GCHandle.Alloc(levels_, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            SharpDX.Direct3D.FeatureLevel level = FeatureLevel.Level_11_0;
            SharpDX.Direct3D12.FeatureDataFeatureLevels levels = new FeatureDataFeatureLevels();
            levels.FeatureLevelCount = levels_.Length;
            levels.FeatureLevelsRequestedPointer = pointer;
            if (_dev.CheckFeatureSupport(SharpDX.Direct3D12.Feature.FeatureLevels, ref levels))
            {
                level = levels.MaxSupportedFeatureLevel;
            }

            pinnedArray.Free();

            System.Console.WriteLine($"  Feature Level: {(((int)level) >> 12) & 0xF}_{(((int)level) >> 8) & 0xF}");
        }

        private void CreateCommandQueue()
        {
            SharpDX.Direct3D12.CommandQueueDescription desc = new CommandQueueDescription
            {
                Type = CommandListType.Direct,
                Priority = (int)CommandQueuePriority.Normal,
                Flags = CommandQueueFlags.None,
                NodeMask = 0
            };

            _queue = _dev.CreateCommandQueue(desc);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Factory5))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SharpDX.DXGI.SwapChain3))]
        private void CreateSwapChain(IntPtr window, bool vsync)
        {
            using (SharpDX.DXGI.Factory4 factory = new Factory4())
            {
                //SharpDX.DXGI.Adapter adapter = factory.GetAdapterByLuid(_dev.AdapterLuid);
                //System.Console.WriteLine($"  Adapter: {adapter.Description.Description}");
                System.Console.WriteLine($"  Adapter: {factory.Adapters[0].Description.Description}");

                if (!vsync)
                {
                    using (SharpDX.DXGI.Factory5 factory5 = factory.QueryInterface<Factory5>())
                    {
                        if (factory5 != null)
                        {
                            SharpDX.Mathematics.Interop.RawBool tearing = false;
                            GCHandle pinnedInt = GCHandle.Alloc(tearing, GCHandleType.Pinned);
                            IntPtr pointer = pinnedInt.AddrOfPinnedObject();

                            factory5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, pointer, System.Runtime.InteropServices.Marshal.SizeOf(tearing));
                            if (tearing != false)
                            {
                                _swapChainFlags = SharpDX.DXGI.SwapChainFlags.AllowTearing;
                                _presentFlags = SharpDX.DXGI.PresentFlags.AllowTearing;
                            }

                            pinnedInt.Free();
                        }
                    }

                    _syncInterval = 0;
                }
                else
                {
                    _syncInterval = 1;
                }

                SharpDX.DXGI.SwapChainDescription1 desc = new SharpDX.DXGI.SwapChainDescription1();
                desc.Width = 0;
                desc.Height = 0;
                desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                desc.Stereo = false;
                desc.SampleDescription = new SampleDescription { Count = 1, Quality = 0 };
                desc.Usage = SharpDX.DXGI.Usage.RenderTargetOutput;
                desc.BufferCount = FrameCount;
                desc.Scaling = SharpDX.DXGI.Scaling.Stretch;
                desc.SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard;
                desc.AlphaMode = SharpDX.DXGI.AlphaMode.Unspecified;
                desc.Flags = _swapChainFlags;

                using (SharpDX.DXGI.SwapChain1 swapChain = new SharpDX.DXGI.SwapChain1(factory, _queue, window, ref desc))
                {
                    factory.MakeWindowAssociation(window, WindowAssociationFlags.IgnoreAltEnter);
                    _swapChain = swapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
                }
            }
        }

        SharpDX.DXGI.SampleDescription GetSampleDescription(uint samples)
        {
            SharpDX.DXGI.SampleDescription[] descs = new SharpDX.DXGI.SampleDescription[16];
            samples = Math.Max(1u, Math.Min(samples, 16u));

            descs[0].Count = 1;
            descs[0].Quality = 0;

            for (uint i = 1, last = 0; i < samples; i++)
            {
                SharpDX.Direct3D12.FeatureDataMultisampleQualityLevels levels = new SharpDX.Direct3D12.FeatureDataMultisampleQualityLevels();
                levels.Format = _format;
                levels.SampleCount = (int)i + 1;
                levels.Flags = SharpDX.Direct3D12.MultisampleQualityLevelFlags.None;
                levels.QualityLevelCount = 0;

                _dev.CheckFeatureSupport(SharpDX.Direct3D12.Feature.MultisampleQualityLevels, ref levels);

                if (levels.QualityLevelCount > 0)
                {
                    descs[i].Count = levels.SampleCount;
                    descs[i].Quality = 0;
                    last = i;
                }
                else
                {
                    descs[i] = descs[last];
                }
            }

            return descs[samples - 1];
        }

        void CreateHeaps()
        {
            _sizeRTV = _dev.GetDescriptorHandleIncrementSize(SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView);

            {
                SharpDX.Direct3D12.DescriptorHeapDescription desc;
                desc.Type = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView;
                desc.DescriptorCount = FrameCount;
                desc.Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None;
                desc.NodeMask = 0;

                _heapRTV = _dev.CreateDescriptorHeap(desc);
            }
            {
                SharpDX.Direct3D12.DescriptorHeapDescription desc;
                desc.Type = SharpDX.Direct3D12.DescriptorHeapType.DepthStencilView;
                desc.DescriptorCount = 1;
                desc.Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None;
                desc.NodeMask = 0;

                _heapDSV = _dev.CreateDescriptorHeap(desc);
            }
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SharpDX.Direct3D12.Resource))]
        void CreateBuffers()
        {
            SharpDX.Direct3D12.CpuDescriptorHandle rtv = _heapRTV.CPUDescriptorHandleForHeapStart;
            SharpDX.Direct3D12.HeapFlags flags = SharpDX.Direct3D12.HeapFlags.None;

            _frameIndex = _swapChain.CurrentBackBufferIndex;
            SharpDX.DXGI.SwapChainDescription1 swapChainDesc = _swapChain.Description1;

            // TODO: Check ID3D12Device8 to enable D3D12_HEAP_FLAG_CREATE_NOT_ZEROED

            SharpDX.Direct3D12.ResourceDescription desc = new SharpDX.Direct3D12.ResourceDescription();
            desc.Dimension = SharpDX.Direct3D12.ResourceDimension.Texture2D;
            desc.Alignment = 0;
            desc.Width = swapChainDesc.Width;
            desc.Height = swapChainDesc.Height;
            desc.DepthOrArraySize = 1;
            desc.MipLevels = 1;
            desc.SampleDescription = _sampleDesc;
            desc.Layout = SharpDX.Direct3D12.TextureLayout.Unknown;

            SharpDX.Direct3D12.HeapProperties heap = new SharpDX.Direct3D12.HeapProperties();
            heap.Type = SharpDX.Direct3D12.HeapType.Default;
            heap.CPUPageProperty = SharpDX.Direct3D12.CpuPageProperty.Unknown;
            heap.MemoryPoolPreference = SharpDX.Direct3D12.MemoryPool.Unknown;
            heap.CreationNodeMask = 0;
            heap.VisibleNodeMask = 0;

            SharpDX.Direct3D12.ClearValue clearValue = new SharpDX.Direct3D12.ClearValue();
            clearValue.Format = _format;
            clearValue.Color = new SharpDX.Mathematics.Interop.RawVector4(0.0f, 0.0f, 0.0f, 0.0f);

            for (int i = 0; i < FrameCount; i++)
            {
                desc.Format = _format;
                desc.Flags = SharpDX.Direct3D12.ResourceFlags.AllowRenderTarget;

                _backBuffers[i] = _swapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);

                if (_sampleDesc.Count != 1)
                {
                    _backBuffersAA[i] = _dev.CreateCommittedResource(heap, flags, desc, SharpDX.Direct3D12.ResourceStates.ResolveSource, clearValue);
                    _dev.CreateRenderTargetView(_backBuffersAA[i], null, rtv);
                }
                else
                {
                    SharpDX.Direct3D12.RenderTargetViewDescription view = new SharpDX.Direct3D12.RenderTargetViewDescription();
                    view.Format = _format;
                    view.Dimension = SharpDX.Direct3D12.RenderTargetViewDimension.Texture2D;
                    view.Texture2D.MipSlice = 0;
                    view.Texture2D.PlaneSlice = 0;

                    _dev.CreateRenderTargetView(_backBuffers[i], view, rtv);
                }

                rtv.Ptr += _sizeRTV;
            }

            // Stencil buffer
            clearValue.Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
            clearValue.DepthStencil.Depth = 0.0f;
            clearValue.DepthStencil.Stencil = 0;

            desc.Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
            desc.Flags = SharpDX.Direct3D12.ResourceFlags.AllowDepthStencil;

            _stencilBuffer = _dev.CreateCommittedResource(heap, flags, desc, SharpDX.Direct3D12.ResourceStates.DepthWrite, clearValue);

            SharpDX.Direct3D12.CpuDescriptorHandle dsv = _heapDSV.CPUDescriptorHandleForHeapStart;
            _dev.CreateDepthStencilView(_stencilBuffer, null, dsv);

            // Viewport
            _viewport[0].TopLeftX = 0.0f;
            _viewport[0].TopLeftY = 0.0f;
            _viewport[0].Width = desc.Width;
            _viewport[0].Height = desc.Height;
            _viewport[0].MinDepth = 0.0f;
            _viewport[0].MaxDepth = 1.0f;

            _scissorRect = new SharpDX.Mathematics.Interop.RawRectangle(0, 0, (int)desc.Width, (int)desc.Height);
        }

        void CreateCommandList()
        {
            // Command allocators
            for (int i = 0; i < FrameCount; i++)
            {
                _commandAllocators[i] = _dev.CreateCommandAllocator(SharpDX.Direct3D12.CommandListType.Direct);
            }

            // Command list
            _commands = _dev.CreateCommandList(0, SharpDX.Direct3D12.CommandListType.Direct, _commandAllocators[0], null);
            _commands.Close();

            // Fence
            _fence = _dev.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
            _fenceEvent = new AutoResetEvent(false);
        }

        public override void SetWindow(IntPtr window) { }

        public override void BeginRender()
        {
            // Wait for the command buffer to be fully processed
            if (_fence.CompletedValue < _frameFenceValues[_frameIndex])
            {
                _fence.SetEventOnCompletion(_frameFenceValues[_frameIndex], _fenceEvent.SafeWaitHandle.DangerousGetHandle());
                _fenceEvent.WaitOne();
            }

            _commandAllocators[_frameIndex].Reset();
            _commands.Reset(_commandAllocators[_frameIndex], null);

            _device.SetCommandList(_commands.NativePointer, _fenceValue + 1);            
        }

        public override void EndRender() { }

        public override void SetDefaultRenderTarget(int width, int height, bool doClearColor)
        {
            SharpDX.Direct3D12.ResourceBarrier barrier = new SharpDX.Direct3D12.ResourceBarrier();
            barrier.Type = SharpDX.Direct3D12.ResourceBarrierType.Transition;
            barrier.Flags = SharpDX.Direct3D12.ResourceBarrierFlags.None;
            if (_sampleDesc.Count > 1)
            {
                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffersAA[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.ResolveSource,
                    SharpDX.Direct3D12.ResourceStates.RenderTarget) { Subresource = 0 };
            }
            else
            {
                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffers[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.Present,
                    SharpDX.Direct3D12.ResourceStates.RenderTarget) { Subresource = 0 };
            }
            _commands.ResourceBarrier(barrier);

            SharpDX.Direct3D12.CpuDescriptorHandle rtv = _heapRTV.CPUDescriptorHandleForHeapStart;
            rtv.Ptr += _frameIndex * _sizeRTV;

            if (doClearColor)
            {
                SharpDX.Mathematics.Interop.RawColor4 clearColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
                _commands.ClearRenderTargetView(rtv, clearColor, 0, null);
            }

            SharpDX.Direct3D12.CpuDescriptorHandle dsv = _heapDSV.CPUDescriptorHandleForHeapStart;
            _commands.ClearDepthStencilView(dsv, SharpDX.Direct3D12.ClearFlags.FlagsStencil, 0.0f, 0, 0, null);
            _commands.SetRenderTargets(1, rtv, dsv);
            _commands.SetViewports(1, _viewport );
            _commands.SetScissorRectangles(_scissorRect);
        }

        public override ImageCapture CaptureRenderTarget(RenderTarget surface)
        {
            IntPtr texNativePtr = RenderDeviceD3D11.GetTextureNativePointer(surface.Texture);
            SharpDX.Direct3D12.Resource texture = new SharpDX.Direct3D12.Resource(texNativePtr);
            SharpDX.Direct3D12.ResourceDescription texDesc = texture.Description;

            // Transition PIXEL_SHADER_RESOURCE -> COPY_SOURCE
            SharpDX.Direct3D12.ResourceBarrier barrier = new SharpDX.Direct3D12.ResourceBarrier();
            barrier.Type = SharpDX.Direct3D12.ResourceBarrierType.Transition;
            barrier.Flags = SharpDX.Direct3D12.ResourceBarrierFlags.None;
            barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(texture,
                SharpDX.Direct3D12.ResourceStates.PixelShaderResource,
                SharpDX.Direct3D12.ResourceStates.CopySource) { Subresource = 0 };

            _commands.ResourceBarrier(barrier);

            long totalBytes;
            long[] rowSize = { 0 };
            _dev.GetCopyableFootprints(ref texDesc, 0, 1, 0, null, null, rowSize, out totalBytes);
            long pitch = (rowSize[0] + 255) & ~255;

            // Create staging texture
            SharpDX.Direct3D12.HeapProperties heap;
            heap.Type = SharpDX.Direct3D12.HeapType.Readback;
            heap.CPUPageProperty = SharpDX.Direct3D12.CpuPageProperty.Unknown;
            heap.MemoryPoolPreference = SharpDX.Direct3D12.MemoryPool.Unknown;
            heap.CreationNodeMask = 0;
            heap.VisibleNodeMask = 0;

            SharpDX.Direct3D12.ResourceDescription desc;
            desc.Dimension = SharpDX.Direct3D12.ResourceDimension.Buffer;
            desc.Alignment = 0;
            desc.Width = pitch * texDesc.Height * 4;
            desc.Height = 1;
            desc.DepthOrArraySize = 1;
            desc.MipLevels = 1;
            desc.Format = SharpDX.DXGI.Format.Unknown;
            desc.SampleDescription = new SampleDescription { Count = 1, Quality = 0 };
            desc.Layout = SharpDX.Direct3D12.TextureLayout.RowMajor;
            desc.Flags = SharpDX.Direct3D12.ResourceFlags.None;

            SharpDX.Direct3D12.Resource stagingTex = _dev.CreateCommittedResource(heap, SharpDX.Direct3D12.HeapFlags.None,
                desc, SharpDX.Direct3D12.ResourceStates.CopyDestination, null);

            SharpDX.Direct3D12.TextureCopyLocation copySrc = new SharpDX.Direct3D12.TextureCopyLocation(texture, 0);
            SharpDX.Direct3D12.TextureCopyLocation copyDst = new SharpDX.Direct3D12.TextureCopyLocation(texture,
                new SharpDX.Direct3D12.PlacedSubResourceFootprint
                {
                    Offset = 0,
                    Footprint = new SharpDX.Direct3D12.SubResourceFootprint
                    {
                        Width = (int)texDesc.Width,
                        Height = (int)texDesc.Height,
                        Depth = 1,
                        RowPitch = (int)pitch,
                        Format = texDesc.Format
                    }
                });

            // Wait GPU completion
            _commands.CopyTextureRegion(copyDst, 0, 0, 0, copySrc, null);
            _commands.Close();
            _queue.ExecuteCommandList(_commands);
            WaitForGpu();

            // Ensure image capture object is created with the appropriate size
            if (_imageCapture == null || _imageCapture.Width != desc.Width || _imageCapture.Height != desc.Height)
            {
                _imageCapture = new ImageCapture((uint)desc.Width, (uint)desc.Height);
            }

            // Map and copy to image
            byte[] dst = _imageCapture.Pixels;
            IntPtr src = stagingTex.Map(0);

            for (int i = 0; i < desc.Height; ++i)
            {
                int dstRow = i * (int)_imageCapture.Stride;
                int srcRow = i * (int)pitch;

                for (int j = 0; j < desc.Width; j++)
                {
                    // RGBA -> BGRA
                    dst[dstRow + 4 * j + 2] = Noesis.Marshal.ReadByte(src, srcRow + 4 * j + 0);
                    dst[dstRow + 4 * j + 1] = Noesis.Marshal.ReadByte(src, srcRow + 4 * j + 1);
                    dst[dstRow + 4 * j + 0] = Noesis.Marshal.ReadByte(src, srcRow + 4 * j + 2);
                    dst[dstRow + 4 * j + 3] = Noesis.Marshal.ReadByte(src, srcRow + 4 * j + 3);
                }
            }

            stagingTex.Unmap(0);

            return _imageCapture;
        }

        public override void Swap()
        {
            SharpDX.Direct3D12.ResourceBarrier barrier = new SharpDX.Direct3D12.ResourceBarrier();
            barrier.Type = SharpDX.Direct3D12.ResourceBarrierType.Transition;
            barrier.Flags = SharpDX.Direct3D12.ResourceBarrierFlags.None;

            if (_sampleDesc.Count > 1)
            {
                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffersAA[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.RenderTarget,
                    SharpDX.Direct3D12.ResourceStates.ResolveSource) { Subresource = 0 };

                _commands.ResourceBarrier(barrier);

                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffers[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.Present,
                    SharpDX.Direct3D12.ResourceStates.ResolveDestination) { Subresource = 0 };

                _commands.ResourceBarrier(barrier);

                _commands.ResolveSubresource(_backBuffers[_frameIndex], 0, _backBuffersAA[_frameIndex], 0, _format);

                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffers[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.ResolveDestination,
                    SharpDX.Direct3D12.ResourceStates.Present) { Subresource = 0 };

                _commands.ResourceBarrier(barrier);
            }
            else
            {
                barrier.Transition = new SharpDX.Direct3D12.ResourceTransitionBarrier(_backBuffers[_frameIndex],
                    SharpDX.Direct3D12.ResourceStates.RenderTarget,
                    SharpDX.Direct3D12.ResourceStates.Present) { Subresource = 0 };

                _commands.ResourceBarrier(barrier);
            }

            // TODO: transition video textures

            _commands.Close();
            _queue.ExecuteCommandList(_commands);

            _fenceValue++;
            _queue.Signal(_fence, _fenceValue);
            _frameFenceValues[_frameIndex] = _fenceValue;

            _swapChain.Present(_syncInterval, _presentFlags);
            _frameIndex = _swapChain.CurrentBackBufferIndex;
        }

        public override void Resize()
        {
            WaitForGpu();

            for (int i = 0; i < FrameCount; i++)
            {
                _backBuffers[i]?.Dispose();
                _backBuffersAA[i]?.Dispose();
            }

            _stencilBuffer?.Dispose();

            _swapChain.ResizeBuffers(0, 0, 0, SharpDX.DXGI.Format.Unknown, _swapChainFlags);

            CreateBuffers();
        }

        void WaitForGpu()
        {
            _fenceValue++;
            _queue.Signal(_fence, _fenceValue);

            if (_fence.CompletedValue < _fenceValue)
            {
                _fence.SetEventOnCompletion(_fenceValue, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
                _fenceEvent.WaitOne();
            }

            for (int i = 0; i < FrameCount; i++)
            {
                _frameFenceValues[i] = 0;
            }
        }
    }
}
