﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Description;

using Microsoft.AspNet.Mvc;

using SocialGamificationAsset.Helpers;
using SocialGamificationAsset.Models;

namespace SocialGamificationAsset.Controllers
{
    [Route("api/items")]
    public class ItemsController : ApiController
    {
        public ItemsController(SocialGamificationAssetContext context)
            : base(context)
        {
        }

        // GET: api/items
        [HttpGet("", Name = "GetItems")]
        public async Task<IList<Item>> GetItems()
        {
            IList<Item> items =
                await _context.Items.Where(i => i.ActorId.Equals(session.Player.Id)).Include(i => i.Type).ToListAsync();

            return items;
        }

        // GET: api/items/types
        [HttpGet("types", Name = "GetItemTypes")]
        public IEnumerable<ItemType> GetItemTypes()
        {
            // TODO: Return Item Types defined for the Game
            return _context.ItemTypes;
        }

        // GET: api/items/type/936da01f-9abd-4d9d-80c7-02af85c822a8
        // GET: api/items/type/gold
        [HttpGet("type/{name}", Name = "GetItemWithCountByType")]
        [ResponseType(typeof(ItemTypeResponse))]
        public async Task<IActionResult> GetItemWithCountByType([FromRoute] string name)
        {
            Guid id;
            var isGuid = Guid.TryParse(name, out id);
            IQueryable<ItemType> query = _context.ItemTypes;
            if (isGuid && id != Guid.Empty)
            {
                query = query.Where(t => t.Id.Equals(id));
            }
            else
            {
                query = query.Where(t => t.Name.Equals(name));
            }

            var type = await query.FirstOrDefaultAsync();
            if (type == null)
            {
                return HttpResponseHelper.BadRequest("Invalid Item Type");
            }

            var total =
                await
                _context.Items.Where(i => i.ActorId.Equals(session.Player.Id))
                        .Where(i => i.ItemTypeId.Equals(type.Id))
                        .GroupBy(i => i.ItemTypeId)
                        .Select(g => new { Total = g.Sum(i => i.Quantity), LastUpdated = g.Max(i => i.UpdatedDate) })
                        .FirstOrDefaultAsync();

            return
                Ok(
                    new ItemTypeResponse
                    {
                        Id = type.Id,
                        Name = type.Name,
                        Image = type.Image,
                        Total = total.Total,
                        LastUpdated = (DateTime)total.LastUpdated
                    });
        }

        // GET: api/items/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpGet("{id:Guid}", Name = "GetItem")]
        [ResponseType(typeof(Item))]
        public async Task<IActionResult> GetItem([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return HttpResponseHelper.BadRequest(ModelState);
            }

            var item = await _context.Items.Where(i => i.Id.Equals(id)).Include(i => i.Type).FirstOrDefaultAsync();
            if (item == null)
            {
                return HttpResponseHelper.NotFound("No such Item found.");
            }

            return Ok(item);
        }

        // GET: api/items/type/936da01f-9abd-4d9d-80c7-02af85c822a8
        [HttpGet("type/{id:Guid}", Name = "GetItemType")]
        [ResponseType(typeof(ItemType))]
        public async Task<IActionResult> GetItemType([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return HttpResponseHelper.BadRequest(ModelState);
            }

            var itemType = await _context.ItemTypes.FindAsync(id);

            if (itemType == null)
            {
                return HttpResponseHelper.NotFound("No such Item Type found.");
            }

            return Ok(itemType);
        }

        // POST: api/items
        [HttpPost]
        [ResponseType(typeof(Item))]
        public async Task<IActionResult> AddItem([FromBody] ItemForm form)
        {
            if (!ModelState.IsValid)
            {
                return HttpResponseHelper.BadRequest(ModelState);
            }

            if ((!form.ItemTypeId.HasValue || form.ItemTypeId == Guid.Empty)
                && string.IsNullOrWhiteSpace(form.ItemTypeName))
            {
                return HttpResponseHelper.BadRequest("Either ItemTypeId or ItemTypeName is required.");
            }

            var searchByName = true;
            IQueryable<ItemType> query = _context.ItemTypes;
            if (form.ItemTypeId.HasValue && form.ItemTypeId != Guid.Empty)
            {
                query = query.Where(t => t.Id.Equals(form.ItemTypeId));
                searchByName = false;
            }

            if (searchByName && !string.IsNullOrWhiteSpace(form.ItemTypeName))
            {
                query = query.Where(t => t.Name.Equals(form.ItemTypeName));
            }

            var itemType = await query.FirstOrDefaultAsync();
            if (itemType == null)
            {
                return HttpResponseHelper.BadRequest("Invalid Item Type.");
            }

            var item = new Item { ItemTypeId = itemType.Id };

            if (!form.ActorId.HasValue || form.ActorId == Guid.Empty)
            {
                item.ActorId = session.Player.Id;
            }

            if (!form.Quantity.HasValue || form.Quantity <= 0)
            {
                item.Quantity = 1;
            }
            else
            {
                item.Quantity = (int)form.Quantity;
            }

            if (form.Operation == ItemOperation.Remove)
            {
                item.Quantity = -item.Quantity;
            }

            _context.Items.Add(item);

            var error = await SaveChangesAsync();
            if (error != null)
            {
                return error;
            }

            return CreatedAtRoute("GetItem", new { id = item.Id }, item);
        }

        // POST: api/items/type
        [HttpPost("type", Name = "AddItemType")]
        [ResponseType(typeof(ItemType))]
        public async Task<IActionResult> AddItemType([FromBody] ItemType itemType)
        {
            if (!ModelState.IsValid)
            {
                return HttpResponseHelper.BadRequest(ModelState);
            }

            var type = await _context.ItemTypes.Where(t => t.Name.Equals(itemType.Name)).FirstOrDefaultAsync();
            if (type != null)
            {
                return HttpResponseHelper.BadRequest("ItemType '" + itemType.Name + "' already exists.");
            }

            _context.ItemTypes.Add(itemType);

            var error = await SaveChangesAsync();
            if (error != null)
            {
                return error;
            }

            return CreatedAtRoute("GetItemType", new { id = itemType.Id }, itemType);
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