using Noesis;

namespace NoesisApp
{
    internal static class DataBindingHelper
    {
        /// <summary>
        /// Ensures that value in the specified dependency property is in sync with binding source
        /// </summary>
        public static void EnsureBindingValue(DependencyObject target, DependencyProperty dp)
        {
            BindingExpression binding = BindingOperations.GetBindingExpression(target, dp);

            if (binding != null)
            {
                binding.UpdateTarget();
            }

        }
    }
}
