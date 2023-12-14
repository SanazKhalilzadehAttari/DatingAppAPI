using DatingAppAPI.Data;
using DatingAppAPI.Interfaces;
using DatingAppAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DatingAppAPI.Extnsions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            /*builder.Services.AddCors(opt=>
            {
                opt.AddDefaultPolicy(policy=> policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));
            });*/
            services.AddScoped<IJWTTokenInterface, JWTTokenService>();
           
            services.AddCors();

            return services;
        }
    }
}
