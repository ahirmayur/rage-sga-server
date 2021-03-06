﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialGamificationAsset.Models
{
    public class Session : DbEntity
    {
        public Session()
        {
            IsExpired = false;
        }

        public Guid PlayerId { get; set; }

        [ForeignKey("PlayerId")]
        public virtual Player Player { get; set; }

        public string LastActionIP { get; set; }

        public bool IsExpired { get; set; }
    }

    public class UserForm
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public IList<CustomDataBase> CustomData { get; set; }
    }
}