using System;
using System.Net;
using System.Collections.ObjectModel;
using System.Web.UI.HtmlControls;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace Custom.SharePoint.ManagedMetadata.Navigation
{
    public static class Navigation
    {

        // Renders the site wide breadcrumb element
        public static void RenderBreadCrumb(HtmlGenericControl breadCrumb)
        {

            // Fetch the child terms of the root term for this page (2nd level)
            var term = GetCurrentPageTerm();

            // Here we're gonna build the html structure
            string structure = "";

            // Walk from current node to root and pick up friendly url segments and titles
            while (term != null)
            {
                if (GetCurrentPageTermId() == term.Id)
                {
                    // For extra security escape the variables for HTML entities. This should not be needed though.
                    structure = "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) + "'><li class='active'>" + WebUtility.HtmlEncode(term.Title.ToString()) + "</li></a>" + structure;
                }
                else
                {
                    // For extra security escape the variables for HTML entities. This should not be needed though.
                    structure = "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) + "'><li>" + WebUtility.HtmlEncode(term.Title.ToString()) + "</li></a>" + structure;
                }
                term = term.Parent;
            }

            // Inject the menu HTML structure
            breadCrumb.InnerHtml = "<ul class='bread-crumb'><a href='/'><li>Home</li></a>" + structure + "</ul>";
        }

        // Renders the structure for the second level menu
        public static void RenderSecondLevelMenu(HtmlGenericControl secondLevelNav)
        {

            // Fetch the child terms of the root term for this page (2nd level)
            var termSet = GetRootNavigationTerm().Terms;

            // Here we're gonna build the html structure
            string structure = "";

            // Loop through all the terms
            foreach (NavigationTerm term in termSet)
            {
                if (GetCurrentPageTermId() == term.Id || (GetCurrentPageTerm().Parent != null && GetCurrentPageTerm().Parent.Id == term.Id))
                {
                    // For extra security escape the variables for HTML entities. This should not be needed though.
                    structure += "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) + "'><li class='active'>" + WebUtility.HtmlEncode(term.Title.ToString()) + "</li></a>";
                }
                else
                {
                    // For extra security escape the variables for HTML entities. This should not be needed though.
                    structure += "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) + "'><li>" + WebUtility.HtmlEncode(term.Title.ToString()) + "</li></a>";
                }

            }

            // Inject the menu HTML structure
            if (structure.Contains("class='active'"))
            {
                secondLevelNav.InnerHtml = "<ul class='second-level-menu'>" + structure + "</ul>";
            }
            else
            {
                secondLevelNav.InnerHtml = "<ul class='second-level-menu inactive'>" + structure + "</ul>";
            }
            
        }

        // Renders the structure for the second level menu
        public static void RenderThirdLevelMenu(HtmlGenericControl thirdLevelNav)
        {
            ReadOnlyCollection<NavigationTerm> termSet = null;

            // If we are at a 3rd level page, just get the sibling terms and render them.
            if (GetCurrentPageLevel() == 3)
            {
                termSet = GetCurrentPageTerm().Parent.Terms;
            }

            // If we are at second level we check if this page has any child terms. If it has, render them.
            else if (GetCurrentPageLevel() == 2 && GetCurrentPageTerm().Terms.Count > 0)
            {
                termSet = GetCurrentPageTerm().Terms;
            }

            if (termSet != null)
            {
                // Here we're gonna build the html structure
                string structure = "";

                // Loop through all the terms
                foreach (NavigationTerm term in termSet)
                {
                    if (GetCurrentPageTermId() == term.Id)
                    {
                        // For extra security escape the variables for HTML entities. This should not be needed though.
                        structure += "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) +
                                     "'><li class='active'>" + WebUtility.HtmlEncode(term.Title.ToString()) +
                                     "</li></a>";
                    }
                    else
                    {
                        // For extra security escape the variables for HTML entities. This should not be needed though.
                        structure += "<a href='" + WebUtility.HtmlEncode(GetTermRelativeUrl(term)) + "'><li>" +
                                     WebUtility.HtmlEncode(term.Title.ToString()) + "</li></a>";
                    }

                }

                // Inject the menu HTML structure
                thirdLevelNav.InnerHtml = "<ul class='third-level-menu'>" + structure + "</ul>";
            }
        }

        // Returns the root term in the term set
        public static NavigationTerm GetRootNavigationTerm()
        {
            NavigationTerm currentTerm = GetCurrentPageTerm();
            NavigationTerm previousTerm = null;

            // Walk from current node to root and count
            while (currentTerm != null)
            {
                previousTerm = currentTerm;
                currentTerm = currentTerm.Parent;
            }

            return previousTerm;
        }

        // Returns the depth of the page in the term set
        public static int GetCurrentPageLevel()
        {
            var term = GetCurrentPageTerm();
            int count = 0;

            // Walk from current node to root and count
            while (term != null)
            {
                count += 1;
                term = term.Parent;
            }

            return count;
        }

        // Get curren page term
        public static NavigationTerm GetCurrentPageTerm()
        {
            TaxonomyNavigationContext tnx = TaxonomyNavigationContext.Current;
            if (tnx.HasFriendlyUrl)
            {
                return tnx.FriendlyUrlTerm;
            }

            return null;
        }

        // Build term url path
        public static string GetTermRelativeUrl(NavigationTerm term)
        {
            string fullRelativePath = "/";

            // Walk from current node to root and pick up friendly url segments
            while (term != null)
            {
                fullRelativePath = "/" + term.FriendlyUrlSegment + fullRelativePath;
                term = term.Parent;
            }

            return fullRelativePath;
        }

        // Get the Guid of the navigation term of the current site
        // Handy for identification purposes
        public static Guid? GetCurrentPageTermId()
        {
            TaxonomyNavigationContext tnx = TaxonomyNavigationContext.Current;
            if (tnx.HasFriendlyUrl)
            {
                return tnx.FriendlyUrlTerm.Id;
            }

            return null;
        }

        // Get child terms of the current page
        public static ReadOnlyCollection<NavigationTerm> GetCurrentPageChildTerms()
        {
            TaxonomyNavigationContext tnx = TaxonomyNavigationContext.Current;
            if (tnx.HasFriendlyUrl)
            {
                return tnx.FriendlyUrlTerm.Terms;
            }

            return null;
        }

        // Finds a termgroup by name (exactly the same name)
        public static Group GetTermGroupByName(GroupCollection groupCollection, string name)
        {
            foreach (var group in groupCollection)
            {
                if (group.Name == name)
                {
                    return group;
                }
            }
            return null;
        }

        // Finds a term set by name (exactly the same name)
        public static TermSet GetTermSetByName(TermSetCollection setCollection, string name)
        {
            foreach (var set in setCollection)
            {
                if (set.Name == name)
                {
                    return set;
                }
            }
            return null;
        }

    }
}
