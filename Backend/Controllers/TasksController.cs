using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Timeclock_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;

        public TasksController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskRepository.GetAll();
            return Ok(tasks);
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        // POST: api/Tasks
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create([FromBody] TaskItem task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.TaskDescription))
            {
                return BadRequest("Task description is required.");
            }

            var newTask = new TaskItem
            {
                TaskDescription = task.TaskDescription.Trim()
            };

            var created = _taskRepository.Add(newTask);
            if (!created)
            {
                return StatusCode(500, "Unable to create task.");
            }

            return CreatedAtAction(nameof(GetById), new { id = newTask.Id }, newTask);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskItem task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.TaskDescription))
            {
                return BadRequest("Task description is required.");
            }

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.TaskDescription = task.TaskDescription.Trim();
            var updated = _taskRepository.Update(existingTask);
            if (!updated)
            {
                return StatusCode(500, "Unable to update task.");
            }

            return Ok(existingTask);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var deleted = _taskRepository.Delete(task);
            if (!deleted)
            {
                return StatusCode(500, "Unable to delete task.");
            }

            return NoContent();
        }
    }
}
