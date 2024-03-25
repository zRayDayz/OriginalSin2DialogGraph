/* This file is part of the MIT C# repository on GitHub: https://github.com/KeRNeLith/GraphShape/
 * Sub-project: https://github.com/KeRNeLith/GraphShape/tree/master/samples/GraphShape.Sample
 * Original author: KeRNeLith (https://github.com/KeRNeLith)
 * License: MIT License
 */
namespace DialogGraph
{
    /// <summary>
    /// Zoom view modes.
    /// </summary>
    internal enum ZoomViewModifierMode
    {
        /// <summary>
        /// It does nothing at all.
        /// </summary>
        None,

        /// <summary>
        /// You can pan the view with the mouse in this mode.
        /// </summary>
        Pan,

        /// <summary>
        /// You can zoom in with the mouse in this mode.
        /// </summary>
        ZoomIn, 

        /// <summary>
        /// You can zoom out with the mouse in this mode.
        /// </summary>
        ZoomOut,

        /// <summary>
        /// Zooming after the user has been selected the zooming box.
        /// </summary>
        ZoomBox
    }
}