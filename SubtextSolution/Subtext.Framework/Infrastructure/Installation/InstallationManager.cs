#region Disclaimer/Info

///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Subtext.Extensibility;
using Subtext.Framework.Components;
using Subtext.Framework.Data;
using Subtext.Framework.Exceptions;
using Subtext.Framework.Providers;
using Subtext.Framework.Services;
using Subtext.Infrastructure;

namespace Subtext.Framework.Infrastructure.Installation
{
    /// <summary>
    /// Class used to help make determine whether an installation is required or not.
    /// </summary>
    public class InstallationManager : IInstallationManager
    {
        public InstallationManager(IInstaller installer, ICache cache)
        {
            Installer = installer;
            Cache = cache;
        }

        protected ICache Cache { get; set; }
        protected IInstaller Installer { get; set; }

        public void Install(Version assemblyVersion)
        {
            Installer.Install(assemblyVersion);
            ResetInstallationStatusCache();
        }

        public void CreateWelcomeContent(ISubtextContext context, IEntryPublisher entryPublisher, Blog blog)
        {
            var repository = context.Repository;
            repository.CreateLinkCategory(new LinkCategory
            {
                Title = "Programming", 
                Description = "Blog posts related to programming", 
                BlogId = blog.Id, 
                IsActive = true, 
                CategoryType = CategoryType.PostCollection
            });
            repository.CreateLinkCategory(new LinkCategory
            {
                Title = "Personal", 
                Description = "Personal musings, random thoughts.", 
                BlogId = blog.Id, 
                IsActive = true, 
                CategoryType = CategoryType.PostCollection
            }
            );

            var entry = new Entry(PostType.BlogPost)
            {
                Title = "Welcome to Subtext!",
                BlogId = blog.Id,
                Author = blog.Author,
                Body = ScriptHelper.UnpackEmbeddedScriptAsString("WelcomePost.htm")
            };
            
            entryPublisher.Publish(entry);
        }

        public void Upgrade(Version currentAssemblyVersion)
        {
            Installer.Upgrade(currentAssemblyVersion);
            ResetInstallationStatusCache();
        }

        /// <summary>
        /// Determines whether an installation action is required by 
        /// examining the specified unhandled Exception.
        /// </summary>
        /// <param name="unhandledException">Unhandled exception.</param>
        /// <param name="assemblyVersion">The version of the currently installed assembly.</param>
        /// <returns>
        /// 	<c>true</c> if an installation action is required; otherwise, <c>false</c>.
        /// </returns>
        public bool InstallationActionRequired(Version assemblyVersion, Exception unhandledException)
        {
            if(unhandledException is HostDataDoesNotExistException)
            {
                return true;
            }

            if(IsInstallationException(unhandledException))
            {
                return true;
            }

            InstallationState status = GetInstallationStatus(assemblyVersion);
            switch(status)
            {
                case InstallationState.NeedsInstallation:
                case InstallationState.NeedsUpgrade:
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInstallationException(Exception exception)
        {
            var tableRegex = new Regex("Invalid object name '.*?'", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            bool isSqlException = exception is SqlException;

            if(isSqlException && tableRegex.IsMatch(exception.Message))
            {
                return true;
            }

            var spRegex = new Regex("'Could not find stored procedure '.*?'", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if(isSqlException && spRegex.IsMatch(exception.Message))
            {
                return true;
            }

            return false;
        }

        public virtual InstallationState GetInstallationStatus(Version currentAssemblyVersion)
        {
            object cachedInstallationState = Cache["NeedsInstallation"];
            if(cachedInstallationState != null)
            {
                return (InstallationState)cachedInstallationState;
            }

            var status = GetInstallationState(currentAssemblyVersion);
            Cache.Insert("NeedsInstallation", status);
            return status;
        }

        private InstallationState GetInstallationState(Version currentAssemblyVersion)
        {
            Version installationVersion = Installer.GetCurrentInstallationVersion();
            if(installationVersion == null)
            {
                return InstallationState.NeedsInstallation;
            }

            if(Installer.NeedsUpgrade(installationVersion, currentAssemblyVersion))
            {
                return InstallationState.NeedsUpgrade;
            }

            return InstallationState.Complete;
        }

        public bool InstallationActionRequired(InstallationState currentState)
        {
            bool needsUpgrade = (currentState == InstallationState.NeedsInstallation
                                 || currentState == InstallationState.NeedsUpgrade);

            return needsUpgrade;
        }

        public void ResetInstallationStatusCache()
        {
            object cachedInstallationState = Cache["NeedsInstallation"];
            if(cachedInstallationState != null)
            {
                Cache.Remove("NeedsInstallation");
            }
        }

        /// <summary>
        /// Determines whether the specified exception is due to a permission 
        /// denied error.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public bool IsPermissionDeniedException(Exception exception)
        {
            var sqlexc = exception.InnerException as SqlException;
            return sqlexc != null
                   &&
                   (
                       sqlexc.Number == (int)SqlErrorMessage.PermissionDeniedInDatabase
                       || sqlexc.Number == (int)SqlErrorMessage.PermissionDeniedOnProcedure
                       || sqlexc.Number == (int)SqlErrorMessage.PermissionDeniedInOnColumn
                       || sqlexc.Number == (int)SqlErrorMessage.PermissionDeniedInOnObject
                   );
        }

    }
}