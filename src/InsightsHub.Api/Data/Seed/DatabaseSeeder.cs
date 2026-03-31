using InsightsHub.Api.Data.Entities;

namespace InsightsHub.Api.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InsightsHubDbContext context)
    {
        if (context.DataSources.Any()) return;

        // ── Data Sources ──────────────────────────────────────────────────────
        var appStore   = Src("src-app-store",   "App Store",     "iOS and Android app reviews",             1);
        var csat       = Src("src-csat",         "CSAT Survey",   "Post-transaction satisfaction surveys",   2);
        var nps        = Src("src-nps",          "NPS Survey",    "Quarterly net promoter score surveys",    3);
        var intercom   = Src("src-intercom",     "Intercom",      "In-app chat and support messages",        4);
        var zendesk    = Src("src-zendesk",      "Zendesk",       "Customer support tickets",                5);
        var salesforce = Src("src-salesforce",   "Salesforce",    "B2B account feedback and cases",          6);
        var slack      = Src("src-slack",        "Slack",         "Internal #voice-of-customer channel",     7);
        var manual     = Src("src-manual",       "Manual",        "Manually entered feedback",               8);

        // ── Tags (type labels + thematic labels combined) ─────────────────────
        var tagBug        = T("Bug");
        var tagFeature    = T("Feature Request");
        var tagUX         = T("UX Issue");
        var tagUrgent     = T("High Priority");
        var tagB2B        = T("B2B");
        var tagPayments   = T("Checkout & Payments");
        var tagSearch     = T("Search Relevance");
        var tagFees       = T("Seller Fees");
        var tagPhotos     = T("Photo Upload");
        var tagWatchlist  = T("Watchlist");
        var tagMessaging  = T("Buyer Messaging");
        var tagLegal      = T("Legal Compliance");
        var tagOnboarding = T("Onboarding");
        var tagPerf       = T("Performance");

        // ── Teams ─────────────────────────────────────────────────────────────
        var teamMarket        = Te("Marketplace");
        var teamPayments      = Te("Payments");
        var teamSearch        = Te("Search & Discovery");
        var teamPropertyB2C   = Te("Property B2C");
        var teamPropertyB2B   = Te("Property B2B");
        var teamMotorsB2C     = Te("Motors B2C");
        var teamMotorsB2B     = Te("Motors B2B");
        var teamJobsB2C       = Te("Jobs B2C");
        var teamJobsB2B       = Te("Jobs B2B");

        // ── Saved Views ───────────────────────────────────────────────────────
        context.SavedViews.AddRange(
            new SavedViewEntity { Name = "All negative feedback", Meta = "Sentiment: Negative" },
            new SavedViewEntity { Name = "Payment issues",        Meta = "Tag: Checkout & Payments" },
            new SavedViewEntity { Name = "B2B feedback",          Meta = "Source: Salesforce · Tag: B2B" }
        );

        context.DataSources.AddRange(appStore, csat, nps, intercom, zendesk, salesforce, slack, manual);
        context.Tags.AddRange(tagBug, tagFeature, tagUX, tagUrgent, tagB2B,
            tagPayments, tagSearch, tagFees, tagPhotos, tagWatchlist,
            tagMessaging, tagLegal, tagOnboarding, tagPerf);
        context.Teams.AddRange(teamMarket, teamPayments, teamSearch,
            teamPropertyB2C, teamPropertyB2B, teamMotorsB2C, teamMotorsB2B, teamJobsB2C, teamJobsB2B);

        await context.SaveChangesAsync();
    }

    private static DataSourceEntity Src(string id, string name, string description, int order) => new()
    {
        Id = id, Name = name, Description = description,
        Status = "connected", LastSynced = "recently", SortOrder = order,
    };

    private static TagEntity T(string name) => new() { Name = name };
    private static TeamEntity Te(string name) => new() { Name = name };

}
