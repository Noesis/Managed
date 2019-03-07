using System;

namespace Noesis
{
    /// <summary>
    ///     Interface through which a layout element (such as a panel) marked
    ///     as an ItemsHost communicates with the ItemContainerGenerator of its
    ///     items owner.
    /// </summary>
    public interface IItemContainerGenerator
    {
        /// <summary>
        /// Return the ItemContainerGenerator appropriate for use by the given panel
        /// </summary>
        ItemContainerGenerator GetItemContainerGeneratorForPanel(Panel panel);

        /// <summary> Prepare the generator to generate, starting at the given position and direction </summary>
        /// <remarks>
        /// This method must be called before calling GenerateNext.  It returns an
        /// IDisposable object that tracks the lifetime of the generation loop.
        /// This method sets the generator's status to GeneratingContent;  when
        /// the IDisposable is disposed, the status changes to ContentReady or
        /// Error, as appropriate.
        /// </remarks>
        IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction);

        /// <summary> Prepare the generator to generate, starting at the given position and direction </summary>
        /// <remarks>
        /// This method must be called before calling GenerateNext.  It returns an
        /// IDisposable object that tracks the lifetime of the generation loop.
        /// This method sets the generator's status to GeneratingContent;  when
        /// the IDisposable is disposed, the status changes to ContentReady or
        /// Error, as appropriate.
        /// </remarks>
        IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem);

        /// <summary> Return the container element used to display the next item. </summary>
        /// <remarks>
        /// This method must be called in the scope of the IDisposable returned by
        /// a previous call to StartAt.
        /// </remarks>
        DependencyObject GenerateNext();

        /// <summary> Return the container element used to display the next item.
        /// When the next item has not been realized, this method returns a container
        /// and sets isNewlyRealized to true.  When the next item has been realized,
        /// this method returns the exisiting container and sets isNewlyRealized to
        /// false.
        /// </summary>
        /// <remarks>
        /// This method must be called in the scope of the IDisposable returned by
        /// a previous call to StartAt.
        /// </remarks>
        DependencyObject GenerateNext(out bool isNewlyRealized);

        /// <summary>
        /// Prepare the given element to act as the ItemUI for the
        /// corresponding item.  This includes applying the ItemUI style,
        /// forwarding information from the host control (ItemTemplate, etc.),
        /// and other small adjustments.
        /// </summary>
        /// <remarks>
        /// This method must be called after the element has been added to the
        /// visual tree, so that resource references and inherited properties
        /// work correctly.
        /// </remarks>
        /// <param name="container"> The container to prepare.
        /// Normally this is the result of the previous call to GenerateNext.
        /// </param>
        void PrepareItemContainer(DependencyObject container);

        /// <summary>
        /// Remove all generated elements.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Remove generated elements.
        /// </summary>
        /// <remarks>
        /// The position must refer to a previously generated item, i.e. its
        /// Offset must be 0.
        /// </remarks>
        void Remove(GeneratorPosition position, int count);

        /// <summary>
        /// Map an index into the items collection to a GeneratorPosition.
        /// </summary>
        GeneratorPosition GeneratorPositionFromIndex(int itemIndex);

        /// <summary>
        /// Map a GeneratorPosition to an index into the items collection.
        /// </summary>
        int IndexFromGeneratorPosition(GeneratorPosition position);
    }
}
