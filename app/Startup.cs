using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace app
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OktaSettings>(Configuration.GetSection("Okta"));
            services.AddSingleton<ITokenService, OktaTokenService>();
            services.AddTransient<IApiService, SimpleApiService>();
            services.AddMvc();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromSeconds(60);
                options.Cookie.Name = "AuthenticationCookie";
                options.Cookie.Expiration = TimeSpan.FromSeconds(60);
                options.Cookie.MaxAge = TimeSpan.FromSeconds(60);

                //options.Validate();
            })
            .AddOpenIdConnect(options => {
                options.Authority = "https://dev-360787.oktapreview.com/oauth2/default";
                options.SignInScheme = "Cookies";
                options.ResponseType = "code id_token";
                options.ClientId = "0oafon9cxvPe2OMFf0h7";
                options.ClientSecret = "TaXLIYMgfzl9pgxsevrn34ozUqBRtRV6wpIm91Vb";
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.UseTokenLifetime = true;
                options.MetadataAddress = "https://dev-360787.oktapreview.com/oauth2/default/.well-known/oauth-authorization-server";
                //options.Configuration.

                //options.Validate();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
