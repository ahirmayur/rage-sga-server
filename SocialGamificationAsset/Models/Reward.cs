﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SocialGamificationAsset.Models
{
	public class Reward : Model
	{
		public Guid AttributeTypeId { get; set; }

		[ForeignKey("AttributeTypeId")]
		public virtual AttributeType AttributeType { get; set; }

		public float Value { get; set; }

		public RewardStatus Status { get; set; }
	}

	public enum RewardStatus
	{
		InProgress,
		Completed
	}
}