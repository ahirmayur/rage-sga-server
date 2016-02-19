﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

using SocialGamificationAsset.Models;

namespace SocialGamificationAsset.Controllers
{
    [Route("api/inventory")]
    public class InventoryController : ApiController
    {
        public InventoryController(SocialGamificationAssetContext context)
            : base(context)
        {
        }

        // GET: api/inventory
        [HttpGet]
        public IEnumerable<Inventory> GetInventory()
        {
            return _context.Inventory;
        }

        // GET: api/inventory/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpGet("{id:Guid}", Name = "GetInventory")]
        public async Task<IActionResult> GetInventory([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Helper.HttpBadRequest(ModelState);
            }

            var inventory = await _context.Inventory.FindAsync(id);

            if (inventory == null)
            {
                return Helper.HttpNotFound("No Inventory found.");
            }

            return Ok(inventory);
        }

        // PUT: api/inventory/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> PutInventory([FromRoute] Guid id, [FromBody] Inventory inventory)
        {
            if (!ModelState.IsValid)
            {
                return Helper.HttpBadRequest(ModelState);
            }

            if (id != inventory.Id)
            {
                return Helper.HttpBadRequest("Invalid Inventory Id.");
            }

            _context.Entry(inventory).State = EntityState.Modified;

            await SaveChangesAsync();

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<IActionResult> PostInventory([FromBody] Inventory inventory)
        {
            if (!ModelState.IsValid)
            {
                return Helper.HttpBadRequest(ModelState);
            }

            _context.Inventory.Add(inventory);

            await SaveChangesAsync();

            return CreatedAtRoute("GetInventory", new { id = inventory.Id }, inventory);
        }

        // DELETE: api/inventory/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteInventory([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Helper.HttpBadRequest(ModelState);
            }

            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory == null)
            {
                return Helper.HttpNotFound("No Inventory found.");
            }

            _context.Inventory.Remove(inventory);

            await SaveChangesAsync();

            return Ok(inventory);
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