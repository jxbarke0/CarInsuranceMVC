using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarInsuranceMVC.Models;
using CarInsuranceMVC.ViewModels;

namespace CarInsuranceMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CarInsurance;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignUp()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Quote()
        {
            ViewBag.Message = "Your quote page.";

            return View();
        }

        [HttpPost]
        public ActionResult GetQuote(string firstName, string lastName, string emailAddress, 
            DateTime dateOfBirth, int carYear, string carMake, string carModel, bool dUI, int ticketCount, bool coverage)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(emailAddress) || carYear == 0 || 
                string.IsNullOrEmpty(carMake) || string.IsNullOrEmpty(carModel))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            else
            {
                //calculating the user's Age
                int userAge = 0;
                userAge = DateTime.Now.Year - dateOfBirth.Year;
                if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear) userAge = userAge - 1;

                //calculating quoteAmount
                double quoteAmount = 50;
                if ((userAge < 25 && userAge > 18) || userAge > 100) quoteAmount += 25;
                if (userAge < 18) quoteAmount += 100;
                if (carYear < 2000 || carYear > 2015) quoteAmount += 25;
                if (carMake == "Porsche" || carMake == "porsche") quoteAmount += 25;
                if ((carMake == "Porsche" || carMake == "porsche") && (carModel == "911 Carrera" || carModel == "911 carrera"))
                    quoteAmount += 25;
                if (ticketCount > 0)
                {
                    quoteAmount += (ticketCount * 10);
                }
                if (dUI == true) quoteAmount += (quoteAmount * .25);
                if (coverage == true) quoteAmount += (quoteAmount * .5);



                

                //insert into database table
                string queryString = @"INSERT INTO Quotes (FirstName, LastName, EmailAddress, DateOfBirth, CarYear, CarMake, CarModel, DUI, TicketCount, Coverage, QuoteAmount) VALUES
                                    (@FirstName, @LastName, @EmailAddress, @DateOfBirth, @CarYear, @CarMake, @CarModel, @DUI, @TicketCount, @Coverage, @QuoteAmount)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.Add("@FirstName", SqlDbType.VarChar);
                    command.Parameters.Add("@LastName", SqlDbType.VarChar);
                    command.Parameters.Add("@EmailAddress", SqlDbType.VarChar);
                    command.Parameters.Add("@DateOfBirth", SqlDbType.DateTime);
                    command.Parameters.Add("@CarYear", SqlDbType.Int);
                    command.Parameters.Add("@CarMake", SqlDbType.VarChar);
                    command.Parameters.Add("@CarModel", SqlDbType.VarChar);
                    command.Parameters.Add("@DUI", SqlDbType.VarChar);
                    command.Parameters.Add("@TicketCount", SqlDbType.Int);
                    command.Parameters.Add("@Coverage", SqlDbType.VarChar);
                    command.Parameters.Add("@QuoteAmount", SqlDbType.Float);

                    command.Parameters["@FirstName"].Value = firstName;
                    command.Parameters["@LastName"].Value = lastName;
                    command.Parameters["@EmailAddress"].Value = emailAddress;
                    command.Parameters["@DateOfBirth"].Value = dateOfBirth;
                    command.Parameters["@CarYear"].Value = carYear;
                    command.Parameters["@CarMake"].Value = carMake;
                    command.Parameters["@CarModel"].Value = carModel;
                    command.Parameters["@DUI"].Value = dUI;
                    command.Parameters["@TicketCount"].Value = ticketCount;
                    command.Parameters["@Coverage"].Value = coverage;
                    command.Parameters["@QuoteAmount"].Value = quoteAmount;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                ViewBag.quoteAmount = quoteAmount;

                return View("Success");
            }
        }

        public ActionResult Admin()
        {
            string queryString = @"SELECT Id, FirstName, LastName, EmailAddress, DateOfBirth, CarYear, CarMake, CarModel, DUI, TicketCount, Coverage, QuoteAmount from Quotes";
            List<QuoteModel> quotes = new List<QuoteModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var quote = new QuoteModel();
                    quote.Id = Convert.ToInt32(reader["Id"]);
                    quote.FirstName = reader["FirstName"].ToString();
                    quote.LastName = reader["LastName"].ToString();
                    quote.EmailAddress = reader["EmailAddress"].ToString();
                    quote.DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]);
                    quote.CarYear = Convert.ToInt32(reader["CarYear"]);
                    quote.CarMake = reader["CarMake"].ToString();
                    quote.CarModel = reader["CarModel"].ToString();
                    quote.DUI = Convert.ToBoolean(reader["DUI"]);
                    quote.TicketCount = Convert.ToInt32(reader["TicketCount"]);
                    quote.Coverage = Convert.ToBoolean(reader["Coverage"]);
                    quote.QuoteAmount = Convert.ToDouble(reader["QuoteAmount"]);
                    quotes.Add(quote);

                }
            }
            var quoteVMs = new List<QuoteVM>();
            foreach (var quote in quotes)
            {
                var quoteVM = new QuoteVM();
                quoteVM.FirstName = quote.FirstName;
                quoteVM.LastName = quote.LastName;
                quoteVM.EmailAddress = quote.EmailAddress;
                quoteVM.QuoteAmount = quote.QuoteAmount;
                quoteVMs.Add(quoteVM);
            }
                return View(quoteVMs);
        }
    }
}