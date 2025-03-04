using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSitePlugin.V2.TwitchApi.CelebrationEmotes;
class SubscriptionProduct
{
}
class Emote
{
    public string AssetType { get; private set; }
    public string Id { get; private set; }
    public string SetId { get; private set; }
    public string Token { get; private set; }
    private Emote(string assetType, string setId, string token)
    {
        AssetType = assetType;
        Id = setId;
        SetId = setId;
        Token = token;
    }
    public static Emote Parse(dynamic json)
    {
        return new Emote(json.assetType, json.setId, json.token);
    }
}