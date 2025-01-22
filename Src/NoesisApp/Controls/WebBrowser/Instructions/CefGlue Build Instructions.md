Clone the repository:

`git clone https://github.com/OutSystems/CefGlue.git`

Checkout the appropriate branch/tag.

`git checkout -b 120.6099.2 tags/120.6099.2`

Apply the patch with our changes

`git apply CefGlue.120.6099.2.patch`

Build the CefGlue nuget

`cd CefGlue`

`dotnet clean`

`dotnet builc -c Release`

`dotnet pack -c Release`