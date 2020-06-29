using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{UserId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettins> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, 
        IOptions<CloudinarySettins> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;
            
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photofromrepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photofromrepo);
            return Ok(photo);
        }
        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userid, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
                
            var userFromRepo = await _repo.GetUser(userid);

            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();

            if(file.Length>0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Url.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if(!userFromRepo.Photos.Any(u => u.IsMain))
            photo.IsMain = true;

            userFromRepo.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                var phototoreturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new {userid = userid, id = photo.Id}, phototoreturn);
            }
            return BadRequest("could not add the photo");
        }
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userid, int id)
        {
            
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userid);
            
            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromrepo = await _repo.GetPhoto(id);
            
            if(photoFromrepo.IsMain)
                return BadRequest("this is already a main photo");
            
            var currentmainphoto = await _repo.GetMainPhotoForUser(userid);
            currentmainphoto.IsMain=false;
            photoFromrepo.IsMain=true;
            
            if(await _repo.SaveAll())
                return NoContent();  
           
            return BadRequest("could not set photo to main");  
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userid,int id)
        {
            if(userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userid);
            
            var photoFromrepo = await _repo.GetPhoto(id);
            
            if(photoFromrepo.IsMain)
                return BadRequest("You cannot delete your main photo");
            
            if(photoFromrepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromrepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                    _repo.Delete(photoFromrepo);

            }

            if(photoFromrepo.PublicId == null)
            {                
                _repo.Delete(photoFromrepo);
            }
            
            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("failed to delete the photo");
            
        }
    }
}