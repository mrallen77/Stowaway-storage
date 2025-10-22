using Microsoft.AspNetCore.Mvc;
using StowawayStorage.Models;
using StowawayStorage.Services;
using System;

namespace StowawayStorage.Controllers
{
    public class ShippingController : Controller
    {
        private readonly USPSShippingService _usps;

        public ShippingController(USPSShippingService usps)
        {
            _usps = usps;
        }

        [HttpGet]
        public IActionResult Estimate() => View(new ShippingEstimate());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Estimate(ShippingEstimate model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.WeightLbs <= 0)
            {
                ModelState.AddModelError(nameof(model.WeightLbs), "Weight must be greater than 0.");
                return View(model);
            }

            // Convert pounds -> ounces (round up to nearest ounce)
            var weightOz = (int)Math.Ceiling(model.WeightLbs * 16.0);

            decimal rate;
            try
            {
                rate = await _usps.GetRateAsync(model.DestinationZip, weightOz);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unable to retrieve USPS rate: {ex.Message}");
                return View(model);
            }

            model.EstimatedCost = (double)rate;
            ViewBag.ResultMessage = $"USPS Priority Mail Estimate: ${rate:F2}";

            return View(model);
        }
    }
}
