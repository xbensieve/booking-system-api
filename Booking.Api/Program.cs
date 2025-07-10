using Booking.Repository.ApplicationContext;
using Booking.Repository.Implementations;
using Booking.Repository.Interfaces;
using Booking.Service.Config;
using Booking.Service.Implementations;
using Booking.Service.Interfaces;
using Booking.Service.Mapping;
using CloudinaryDotNet;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

namespace Booking.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;
            DotNetEnv.Env.Load();
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(Path.Combine(builder.Environment.ContentRootPath, "firebase-adminsdk.json"))
                });
            }
            builder.Services.AddMemoryCache();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IHotelService, HotelService>();
            builder.Services.AddScoped<IHotelImageService, HotelImageService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                    builder.WithOrigins("http://localhost:8080")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });
            var cloudinarySettings = config.GetSection("Cloudinary").Get<CloudinarySettings>();
            if (cloudinarySettings == null || string.IsNullOrEmpty(cloudinarySettings.CloudName) ||
                string.IsNullOrEmpty(cloudinarySettings.ApiKey) || string.IsNullOrEmpty(cloudinarySettings.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing or invalid.");
            }
            var cloudinary = new Cloudinary(new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            ));
            builder.Services.AddSingleton(cloudinary);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Booking API",
                    Version = "v1",
                    Description = "API documentation for the Booking system."
                });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter your Firebase ID token here using the format: **Bearer &lt;token&gt;**"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header
                        },
                    Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            //app.UseMiddleware<FirebaseAuthMiddleware>();
            //app.UseMiddleware<CsrfValidationMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
