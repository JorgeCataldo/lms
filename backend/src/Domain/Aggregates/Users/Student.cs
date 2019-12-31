﻿namespace Domain.Aggregates.Users
{
    public class Student: User
    {
        public Student(string userName, string name, string email, string normalizedUsername, bool isBlocked) : base(userName, name, email, normalizedUsername, isBlocked)
        {
            
        }
        
        public override string GetUserRole(User user)
        {
            return "Student";
        }
    }
}