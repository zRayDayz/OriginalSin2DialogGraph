using System.Diagnostics.CodeAnalysis;

/* This file is part of the MIT C# repository on GitHub: https://github.com/KeRNeLith/GraphShape/
 * Sub-project: https://github.com/KeRNeLith/GraphShape/tree/master/samples/GraphShape.Sample
 * Original author: KeRNeLith (https://github.com/KeRNeLith)
 * License: MIT License
 */
namespace DialogGraph
{
    /// <summary>
    /// Handler for a content size changed event.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="newSize">New content size.</param>
    internal delegate void ContentSizeChangedHandler([NotNull] object sender, System.Windows.Size newSize);
}