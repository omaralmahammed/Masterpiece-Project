using CodersBackEnd.DTO;
using CodersBackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;

namespace CodersBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly MyDbContext _db;


        public PaymentController(MyDbContext db)
        {
            _db = db;

            StripeConfiguration.ApiKey = "sk_test_51Q3FzBRqxwpgnuaX7azGSStPP6UpFrrMOYsg51jX6Tkoj2M4q95UWWxkWvy8DuIdyVcav2EOZxtXf5O5wMhWDxQC003Xx9VDhk";
        }

        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession( [FromBody] PaymentRequestDTO paymentRequest)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = paymentRequest.ProductName,
                        },
                        UnitAmount = long.Parse(paymentRequest.Amount)
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    SuccessUrl = paymentRequest.SuccessUrl,
                    CancelUrl = paymentRequest.CancelUrl,
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }


        [HttpPost("AddPaymentInfo")]
        public IActionResult AddPaymentInfo([FromBody] PaymentRequestDTO paymentRequest)
        {
            var checkStudentPayment = _db.Payments.Where(u => u.UserId == paymentRequest.UserId).FirstOrDefault();

            if (checkStudentPayment != null)
            {
                return BadRequest("student added before!");
            }

            Payment newPayment = new Payment()
            {
                Amount = paymentRequest.Amount,
                PaymentMethod = "Visa",
                PaymentStatus = "Sucsses",
                PaymentDate = DateTime.Now,
                UserId = paymentRequest.UserId,
                ProgramId = paymentRequest.ProgramId
            };

            _db.Payments.Add(newPayment);
            _db.SaveChanges();

            return Ok();
        }


        [HttpPost("AddStudent")]
        public IActionResult AddStudent([FromBody] PaymentRequestDTO paymentRequest)
        {
            var check = _db.Students.Where(s => s.UserId == paymentRequest.UserId).FirstOrDefault();
            
            if(check != null)
            {
                return BadRequest("Student Already Subscribed!");
            }


            Student newStudent = new Student()
            {
                UserId = paymentRequest.UserId,
                ProgramId = paymentRequest.ProgramId
            };

            _db.Students.Add(newStudent);
            _db.SaveChanges();
            return Ok();
        }


        [HttpPut("AddBllingDetails/{userId}")]
        public IActionResult AddBllingDetails([FromForm] BillingRequestDTO updateInfo, int userId)
        {
            var studentPymentInfo = _db.BillingDetails.Where(u => u.UserId == userId).FirstOrDefault();

            studentPymentInfo.FirstName = updateInfo.FirstName;
            studentPymentInfo.LastName = updateInfo.LastName;
            studentPymentInfo.Address = updateInfo.Address;
            studentPymentInfo.City = updateInfo.City;
            studentPymentInfo.County = updateInfo.County;
            studentPymentInfo.Postcode = updateInfo.Postcode;


            _db.BillingDetails.Update(studentPymentInfo);
            _db.SaveChanges();
            return Ok();

        }

        [HttpGet("GetBillingDetails/{userId}")]
        public IActionResult GetBillingDetails(int userId) {

            var userBillingDetails = _db.BillingDetails.Where(s => s.UserId == userId).FirstOrDefault();

            return Ok(userBillingDetails); 
                
        }
    }




}

