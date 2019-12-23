namespace Domain.Aggregates.Users
{
    public class BusinessManager : User
    {
        public BusinessManager(string userName, string name, string email, string normalizedUsername, bool isBlocked) : base(userName, name, email, normalizedUsername, isBlocked)
        {
            
        }
        
        public override string GetUserRole(User user)
        {
            return "BusinessManager";
        }
    }
}