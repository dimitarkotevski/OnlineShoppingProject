namespace NexCart.Business.Exceptions.ExceptionMassages
{
    public static class ShopExceptionMassages
    {

        public static class UserExceptionMassages
        {
            public static string ID_NOT_FOUND_EXCEPTION = "User with Id: {0} not found";
            public static string NOT_FOUND_EXCEPTION = "User not found";
            public static string USERNAME_PASSWORD_INCORRECT = "Username or password are incorrect";
            public static string SAME_OLD_AND_NEW_PASWORD = "Old password and new password are the same.";

            public static string EMAIL_ALREADY_EXIST = "Email '{0}'  already exist";
            public static string EMAIL_NOT_FOUND_EXCEPTION = "User with email: '{0}' not found";

            public static string USERNAME_ALREADY_EXIST = "Username '{0}'  already exist";
            public static string USERNAME_NOT_FOUND_EXCEPTION = "User with username: '{0}' not found";

            public static string PASSWORD_IS_INCORRECT = "Password is incorrect";

            public static string USERNAME_IS_REQUIRED = "Username is required";
            public static string PASSWORD_IS_REQUIRED = "Password is required";

            public static string NOT_AMOUNT_OF_MONEY = "Not amount of money account";
        }
        public static class CategoriesExceptionMassages
        {
            public static string NOT_FOUND_EXCEPTION = "Category not found";
            public static string ID_NOT_FOUND_EXCEPTION = "Category with Id: {0} not found";

            public static string NAME_NOT_FOUND_EXCEPTION = "Category name not exist";
            public static string NAME_ALREADY_EXIST_EXCEPTION = "Category name already exist";
        }
        public static class ProductsExceptionMassages
        {
            public static string NOT_FOUND_EXCEPTION = "Product not found";
            public static string ID_NOT_FOUND_EXCEPTION = "Product with Id: {0} not found";
            public static string NAME_NOT_FOUND_EXCEPTION = "Product name doesn't exist";
            public static string NAME_ALREADY_EXIST_EXCEPTION = "Product name already exist";
        }
        public static class OrderExceptionMassages
        {
            public static string NOT_FOUND_EXCEPTION = "Order not found";
            public static string ID_NOT_FOUND_EXCEPTION = "Order with Id: {0} not found";

            public static string PRODUCT_NOT_FOUND_EXCEPTION = "Product not found exception";
            public static string USER_NOT_FOUND_EXCEPTION = "User not found exception";
        }
        public static class FavouriteProductExceptionMassages
        {
            public static string NOT_FOUND_EXCEPTION = "Favourite product not found";
        }
        public static class ShoppingCartExceptionMassages
        {
            public static string NOT_FOUNG = "Shopping cart not found";
            public static string DETAIL_NOT_FOUND_EXCEPTION = "Shopping detail not found";
        }
        public static class PaymentExceptionMassages
        {
            public static string NOT_FOUND = "Payment not found";
        }
    }
}
