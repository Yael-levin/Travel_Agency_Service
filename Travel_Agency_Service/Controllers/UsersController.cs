using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Travel_Agency_Service.Helpers;
using Travel_Agency_Service.Models;


namespace Travel_Agency_Service.Controllers
{
    public class UsersController : Controller
    {
        private readonly string _connStr;

        public UsersController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                ViewBag.Error = "Invalid email format";
                return View();
            }


            using SqlConnection con = new SqlConnection(_connStr);
            string sql = "SELECT * FROM Users WHERE Email=@email AND IsActive=1";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@email", email);

            con.Open();
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string passwordHashFromDb = reader["PasswordHash"].ToString();

                bool isValidPassword =
                    PasswordHelper.VerifyPassword(passwordHashFromDb, password);

                if (!isValidPassword)
                {
                    ViewBag.Error = "Invalid login";
                    return View();
                }


                string role = reader["Role"].ToString();

                HttpContext.Session.SetString("UserName", reader["FullName"].ToString());
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetInt32("UserId", (int)reader["UserId"]);
                HttpContext.Session.SetString("UserEmail", reader["Email"].ToString());

                return RedirectToAction("Trips", "Trips");

            }

            ViewBag.Error = "Invalid login";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= REGISTER =================

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        public IActionResult Register(string fullName, string email, string password)
        {
            // 1️⃣ שדות ריקים
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            // 2️⃣ תקינות מייל
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                ViewBag.Error = "Invalid email format";
                return View();
            }

            using SqlConnection con = new SqlConnection(_connStr);

            // 3️⃣ מייל כבר קיים ❗
            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Email=@email", con);
            checkCmd.Parameters.AddWithValue("@email", email);

            con.Open();
            int exists = (int)checkCmd.ExecuteScalar();
            if (exists > 0)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            // 4️⃣ בדיקת סיסמה (בלי special char)
            if (password.Length < 6 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsDigit))
            {
                ViewBag.Error =
                    "Password must be at least 6 characters and include an uppercase letter and a number";
                return View();
            }

            // 5️⃣ יצירת משתמש
            string hash = PasswordHelper.HashPassword(password);

            SqlCommand insertCmd = new SqlCommand(
                @"INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive)
          VALUES (@fullName, @email, @hash, 'User', 1)", con);

            insertCmd.Parameters.AddWithValue("@fullName", fullName);
            insertCmd.Parameters.AddWithValue("@email", email);
            insertCmd.Parameters.AddWithValue("@hash", hash);

            insertCmd.ExecuteNonQuery();

            return RedirectToAction("Login");
        }




        // to fix the hash 1 timer!!!!!
        /*
        public IActionResult FixPasswords()
        {
            using SqlConnection con = new SqlConnection(_connStr);
            string sql = "SELECT UserId, PasswordHash FROM Users";

            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();

            var reader = cmd.ExecuteReader();
            List<(int id, string pass)> users = new();

            while (reader.Read())
            {
                users.Add((
                    (int)reader["UserId"],
                    reader["PasswordHash"].ToString()
                ));
            }

            reader.Close();

            foreach (var u in users)
            {
                string newHash = PasswordHelper.HashPassword(u.pass);

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE Users SET PasswordHash=@hash WHERE UserId=@id", con);

                updateCmd.Parameters.AddWithValue("@hash", newHash);
                updateCmd.Parameters.AddWithValue("@id", u.id);

                updateCmd.ExecuteNonQuery();
            }

            return Content("Passwords fixed");
        }
        */
    }
}
