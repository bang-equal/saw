using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using saw.Models;
using saw.Repositories;
using saw.Repositories.Interfaces;
using saw.Security;
using saw.Migrations;

namespace saw
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IArticleRepository, ArticleRepository>();
            
            services.AddEntityFrameworkNpgsql()
               .AddDbContext<ApplicationDbContext>(
                 //Prod gets connection string from appsettings.json
                 opts => opts.UseNpgsql(Environment.GetEnvironmentVariable("DefaultConnection")));

            

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    //When this line commented, got invalid token audience error
                    ValidAudience = Environment.GetEnvironmentVariable("Audience"),
                    ValidIssuer = Environment.GetEnvironmentVariable("Issuer"),
                    // When receiving a token, check that we've signed it.
                    ValidateIssuerSigningKey = true,
                    //PRODUCTION USES ENV VAR
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("SecretKey"))),
                    // When receiving a token, check that it is still valid.
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                    // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                    // machines which should have synchronised time, this can be set to zero. Where external tokens are
                    // used, some leeway here could be useful.
                    ClockSkew = TimeSpan.FromMinutes(0)

                };

            }).AddIdentityCookies(o => { });
            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials() );
            });

            services.AddMvc();

            services.Configure<TokenAuthOption>(options =>
            {
                options.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("SecretKey"))),
                SecurityAlgorithms.HmacSha256Signature);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("CorsPolicy");

            app.UseMvc();

            var articleData = "[{\"ArticleId\": \"1\",\"ArticleTitle\": \"How To Floss\",\"ArticleText\": \"Stand with your knees slightly bent and swing your arms to the left...\"},{\"ArticleId\": \"2\",\"ArticleTitle\": \"How To Best Mates\",\"ArticleText\": \"BStretch arms out and bend elbow leaving fingers pointing downwards...\"},{\"ArticleId\": \"3\",\"ArticleTitle\": \"How To Shoot\",\"ArticleText\": \"Jump on your left leg, swing your right leg back and forth...\"}]";

            //Create DB on startup
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                 var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                 context.Database.Migrate();
                 context.EnsureSeedData(articleData);
            }
        }
    }
}
