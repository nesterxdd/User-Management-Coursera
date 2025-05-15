using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace User_Management_API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private static List<User> users = new List<User>();

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        // GET: api/<UserController>
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(users);
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            try
            {
                var user = users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                   
                    return Ok(user);
                }
                else
                {
                    return NotFound("No such user has been found");
                }
            }
            catch
            {
                return Problem("An error occurred while retrieving the user.");
            }

        }

        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            Response.Headers.Append("X-Api-KEY", "secure-token");
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
                users.Add(user);
                return Created($"/api/user/{user.Id}", user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return Problem("An error occurred while creating the user.");
            }
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] User user)
        {
            try
            {
                var updateUser = users.FirstOrDefault(u => u.Id == id);
                if (updateUser != null && user != null)
                {
                    updateUser.Email = user.Email;
                    updateUser.Name = user.Name;
                    updateUser.PhoneNumber = user.PhoneNumber;
                    return Ok(updateUser);
                }
                else
                {
                    return BadRequest("User cannot be null or user not found. Please check the user ID and try again.");
                }
            }
            catch
            {
                return Problem("An error occurred while updating the user.");
            }
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var user = users.FirstOrDefault(u => u.Id == id);
                if(user != null)
                {
                    users.Remove(user);
                    return Ok("User deleted successfully");
                }
                else
                {
                    return NotFound("No such user has been found");
                }
            }
            catch
            {
                return Problem("An error occurred while deleting the user.");
            }
        }
    }
}
