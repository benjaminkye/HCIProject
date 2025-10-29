using System;
using System.Collections.Generic;

namespace DigitalHelper.Models
{
    public class VaultData
    {
        public List<LoginItem> Logins { get; set; } = new List<LoginItem>();
        public List<CardItem> Cards { get; set; } = new List<CardItem>();
        public List<SecureNoteItem> SecureNotes { get; set; } = new List<SecureNoteItem>();
    }

    public class LoginItem
    {
        public string SiteName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CardItem
    {
        public string CardName { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty; // Visa, MasterCard, AmEx, Discover
        public string BillingAddress { get; set; } = string.Empty;

        public string LastFourDigits
        {
            get
            {
                if (string.IsNullOrEmpty(CardNumber) || CardNumber.Length < 4)
                    return "••••";
                return "•••• " + CardNumber.Substring(CardNumber.Length - 4);
            }
        }
    }

    public class SecureNoteItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string ContentPreview
        {
            get
            {
                if (string.IsNullOrEmpty(Content))
                    return "(empty note)";
                
                // Get first 50 characters or first line, whichever is shorter
                var preview = Content.Length > 50 ? Content.Substring(0, 50) : Content;
                var firstLineEnd = preview.IndexOf('\n');
                if (firstLineEnd > 0)
                    preview = preview.Substring(0, firstLineEnd);
                
                return preview.Trim() + (Content.Length > preview.Length ? "..." : "");
            }
        }

        public string FormattedModifiedDate
        {
            get
            {
                var now = DateTime.Now;
                var diff = now - ModifiedDate;

                if (diff.TotalMinutes < 1)
                    return "Just now";
                if (diff.TotalHours < 1)
                    return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalDays < 1)
                    return $"{(int)diff.TotalHours}h ago";
                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays}d ago";
                
                return ModifiedDate.ToString("MMM d, yyyy");
            }
        }
    }
}

