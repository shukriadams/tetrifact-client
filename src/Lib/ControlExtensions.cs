using Avalonia.Controls;

namespace TetrifactClient.Lib
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Traverses parent control heirarchy to find first parent that is window. Returns mainwindow if no other parent found.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static Window GetOwnerWindow(this Control control)
        {
            Window owner = null;
            Control current = control;

            while (current != null)
            {
                if (current is Window)
                {
                    owner = current as Window;
                    break;
                }

                current = current.Parent as Control;
            }

            return owner == null ? MainWindow.Instance : owner;
        }
    }
}
