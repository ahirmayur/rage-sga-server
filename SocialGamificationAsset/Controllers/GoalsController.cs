﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Description;

using Microsoft.AspNet.Mvc;

using SocialGamificationAsset.Models;

namespace SocialGamificationAsset.Controllers
{
    [Route("api/goals")]
    public class GoalsController : ApiController
    {
        public GoalsController(SocialGamificationAssetContext context)
            : base(context)
        {
        }

        // GET: api/goals
        [HttpGet]
        [ResponseType(typeof(IList<Goal>))]
        public async Task<IActionResult> GetGoals()
        {
            IList<Goal> goals =
                await
                _context.ActorGoal.Where(g => g.ActorId.Equals(session.Player.Id))
                        .Include(g => g.Goal)
                        .Select(g => g.Goal)
                        .ToListAsync();

            return Ok(goals);
        }

        // GET: api/goals/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpGet("{id}", Name = "GetGoal")]
        [ResponseType(typeof(Goal))]
        public async Task<IActionResult> GetGoal([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return HttpNotFound();
            }

            return Ok(goal);
        }

        // PUT: api/goals/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpPut("{id}")]
        [ResponseType(typeof(Goal))]
        public async Task<IActionResult> PutGoal([FromRoute] Guid id, [FromBody] Goal goal)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != goal.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(goal).State = EntityState.Modified;

            await SaveChangesAsync();

            return CreatedAtRoute("GetGoal", new { id = goal.Id }, goal);
        }

        // POST: api/goals
        [HttpPost]
        [ResponseType(typeof(Goal))]
        public async Task<IActionResult> PostGoal([FromBody] Goal goal)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Goals.Add(goal);

            await SaveChangesAsync();

            return CreatedAtRoute("GetGoal", new { id = goal.Id }, goal);
        }

        // DELETE: api/goals/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpDelete("{id}")]
        [ResponseType(typeof(Goal))]
        public async Task<IActionResult> DeleteGoal([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return HttpNotFound();
            }

            _context.Goals.Remove(goal);

            await SaveChangesAsync();

            return Ok(goal);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}