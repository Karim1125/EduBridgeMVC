using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Authentication;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services;
using EduBridgeMVC.Services.Interfaces;
using EduBridgeMVC.Settings;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace EduBridgeMVCMVC;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddDependencies(IConfiguration config)
        {
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddServicesConfig();
            services.AddAuthConfig(config);
            services.AddMapsterConfig();
            services.AddFluentValidationConfig();

            var connectionString = config.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.Configure<MailSettings>(config.GetSection(nameof(MailSettings)));
            services.Configure<SecurityCodeSettings>(config.GetSection(SecurityCodeSettings.SectionName));
        }

        private void AddMapsterConfig()
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton<IMapper>(new Mapper(mappingConfig));
        }

        private void AddFluentValidationConfig()
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void AddAuthConfig(IConfiguration config)
        {
            services.AddSingleton<IJwtProvider, JwtProvider>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations();

            var jwtSettings = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();

                        if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                        {
                            context.Token = token.Replace("Bearer ", "");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminPolicy", b => b.RequireRole(DefaultRoles.Admin));
            });
        }

        private void AddServicesConfig()
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailSender, EmailService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IIdeaCategoryService, IdeaCategoryService>();
            services.AddScoped<IIdeaTagService, IdeaTagService>();
            services.AddScoped<IIdeaService, IdeaService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IDoctorRequestService, DoctorRequestService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IJoinRequestService, JoinRequestService>();
            services.AddScoped<ITaRequestService, TaRequestService>();
            services.AddScoped<ITaService, TaService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHttpContextAccessor();
        }
    }
}