﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialGamificationAsset.Models
{
    public class Target : DbEntity
    {
        public Guid AttributeTypeId { get; set; }

        [ForeignKey("AttributeTypeId")]
        public virtual AttributeType AttributeType { get; set; }

        public float Value { get; set; }

        public RewardStatus Status { get; set; }

        public Direction Operation { get; set; }

        public Guid GoalId { get; set; }

        [ForeignKey("GoalId")]
        public virtual Goal Goal { get; set; }
    }
}