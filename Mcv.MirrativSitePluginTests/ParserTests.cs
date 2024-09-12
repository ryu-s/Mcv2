using Mcv.PluginV2;
using MirrativSitePlugin;
using NUnit.Framework;
using System;

namespace MirrativSitePluginTests
{
    [TestFixture]
    class ParserTests
    {
        [Test]
        public void Test()
        {
            var data = "{\"push_image_url\":\"\",\"speech\":\"\",\"d\":1,\"ac\":\"Mirrativ bot\",\"burl\":\"https://www.mirrativ.com/assets/img/ic_badge_S.png?v2\",\"iurl\":\"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/ce6c9a48c7d08228af072c7de32fc750f237311c0755f95a7693c88e27cf1d90_m.jpeg?1508489473\",\"cm\":\"シェイク検知：60秒間、画面共有を停止するよ。再シェイクすると画面共有を再開できるよ！\",\"created_at\":1546438220,\"u\":\"1540862\",\"is_moderator\":0,\"lci\":1331546385,\"t\":1}";
            var json = Codeplex.Data.DynamicJson.Parse(data);
            var message = MirrativSitePlugin.Tools.ParseType1Data(json);
            Assert.That("シェイク検知：60秒間、画面共有を停止するよ。再シェイクすると画面共有を再開できるよ！", Is.EqualTo(message.Comment));
            Assert.That(1546438220, Is.EqualTo(message.CreatedAt));
            Assert.That("1331546385", Is.EqualTo(message.Id));
            Assert.That("1540862", Is.EqualTo(message.UserId));
            Assert.That("Mirrativ bot", Is.EqualTo(message.Username));
            Assert.That(MessageType.Comment, Is.EqualTo(message.Type));
        }
        [Test]
        public void Test1()
        {
            var data = "{\"gift_title\":\"かわいいエモモスナップ(300)\",\"photo_gift_id\":\"9162721\",\"burl\":\"\",\"coins\":\"300\",\"gift_small_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/small_64.png?v=5\",\"u\":\"4353835\",\"nameplate_enabled\":\"1\",\"t\":35,\"avatar_user_ids\":\"4072373,4383477,6221780,4353835,2921078,664329\",\"count\":1,\"is_photo_gift\":1,\"ac\":\"matsu【\\ud83c\\udfa8定期組】\\ud83c\\udf77\\ud83c\\udccf\\ud83d\\udc9c \",\"total_gift_coins\":\"25972\",\"iurl\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/profile_image\\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_m.jpeg?1552123860\",\"gift_id\":\"64\",\"pause_duration\":\"0\",\"orientations\":\"0\",\"gift_large_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/large_64.png?v=5\",\"photo_gift_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/photo_gift:1552124210:4353835:26477211\\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_origin.png?1552124211\",\"share_text\":\"@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ\\ud83c\\udfa8さんとの  #エモモスナップ！ #エモモ #ミラティブ\"}";
            MessageParser.GetCurrent = () => new DateTime(2019, 12, 9, 1, 2, 3);
            var message = MessageParser.ParseMessage(data, (msg, type) => { });
            var photoGift = message as MirrativPhotoGift;
            Assert.That(photoGift?.BUrl, Is.Null);
            Assert.That(photoGift?.Coins, Is.EqualTo(300));
            Assert.That(photoGift?.Text, Is.EqualTo("@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ🎨さんとの  #エモモスナップ！ #エモモ #ミラティブ"));
            Assert.That(photoGift?.GiftSmallImageUrl, Is.Null);
            Assert.That(photoGift?.GiftTitle, Is.EqualTo("かわいいエモモスナップ(300)"));
            Assert.That(photoGift?.Id, Is.Null);
            Assert.That(photoGift?.MirrativMessageType, Is.EqualTo(MirrativMessageType.Item));
            Assert.That(photoGift?.Text, Is.EqualTo("@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ🎨さんとの  #エモモスナップ！ #エモモ #ミラティブ"));
            Assert.That(photoGift?.PhotoGiftId, Is.Null);
            Assert.That(photoGift?.PostedAt, Is.EqualTo(new DateTime(2019, 12, 9, 1, 2, 3)));
            Assert.That(photoGift?.ShareText, Is.EqualTo("@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ🎨さんとの  #エモモスナップ！ #エモモ #ミラティブ"));
            Assert.That(photoGift?.SiteType, Is.EqualTo(SiteType.Mirrativ));
            Assert.That(photoGift?.UserId, Is.EqualTo("4353835"));
        }
        [Test]
        public void Test2()
        {
            var data = "{\"count\":\"8\",\"gift_title\":\"小さな星\",\"total_gift_coins\":\"26306\",\"ac\":\"\\ud83d\\udc3e真顔ちゃん'-'\\ud83c\\udf4a\\ud83c\\udf4c\\ud83d\\udd4a\\ud83d\\udc36\\ud83c\\udf31\\ud83c\\udf75\",\"burl\":\"\",\"iurl\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/image\\/profile_image\\/fa3a29a81ece745badebc1fee44071997da131414ee7d53e2bb5228f2adf23cd_m.jpeg?1551797451\",\"coins\":\"1\",\"gift_small_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/small_1.png?v=2\",\"u\":\"5101297\",\"gift_id\":\"1\",\"nameplate_enabled\":\"1\",\"pause_duration\":\"0\",\"gift_large_image_url\":\"https:\\/\\/cdn.mirrativ.com\\/mirrorman-prod\\/assets\\/img\\/gift\\/large_1.png?v=2\",\"t\":35}";
            MessageParser.GetCurrent = () => new DateTime(2019, 12, 9, 1, 0, 0);
            var message = MessageParser.ParseMessage(data, (msg, type) => { });
            var gift = message as MirrativGift;
            Assert.That(gift, Is.Not.Null);
            Assert.That(gift.Count, Is.EqualTo(8));
            Assert.That(gift.Text, Is.EqualTo("🐾真顔ちゃん'-'🍊🍌🕊🐶🌱🍵が小さな星を8個贈りました"));
            Assert.That(gift.UserName, Is.EqualTo("🐾真顔ちゃん'-'🍊🍌🕊🐶🌱🍵"));
        }
        /// <summary>
        /// 小さなハート
        /// </summary>
        [Test]
        public void SmallHeartTest()
        {
            var data = "{\"avatar_body_image_url\":\"\",\"gift_title\":\"小さなハート\",\"speech\":\"さぶさぶはらぐちが小さなハートを贈りました\",\"should_play_animation\":1,\"gift_type\":\"0\",\"burl\":\"https://cdn.mirrativ.com/assets/img/continuous_streamer/ic_badge_master_holiday_L.ja.png?v1\",\"coins\":\"1\",\"gift_small_image_url\":\"https://cdn.mirrativ.com/mirrorman-prod/assets/gift/item/2/small.png?v=207\",\"u\":\"110555163\",\"nameplate_enabled\":\"1\",\"is_moderator\":0,\"t\":35,\"count\":\"1\",\"collab_streamer_ac\":\"\",\"ac\":\"さぶさぶはらぐち\",\"total_gift_coins\":\"121\",\"iurl\":\"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/0d12278c7ce066c7d084f1958ecb60222c07217f15e378bf27916d4c84f410aa_m.jpeg?1618843177\",\"live_sent_gift_id\":\"360927124\",\"gift_id\":\"2\",\"pause_duration\":\"0\",\"slot_id\":0,\"gift_large_image_url\":\"https://cdn.mirrativ.com/mirrorman-prod/assets/gift/item/2/large.png?v=207\"}";
            MessageParser.GetCurrent = () => new DateTime(2019, 12, 9, 1, 0, 0);
            var message = MessageParser.ParseMessage(data, (msg, type) => { });
            var gift = message as MirrativGift;
            Assert.That(gift, Is.Not.Null);
            Assert.That(gift.Count, Is.EqualTo(1));
            Assert.That(gift.Text, Is.EqualTo("さぶさぶはらぐちが小さなハートを1個贈りました"));
            Assert.That(gift.UserName, Is.EqualTo("さぶさぶはらぐち"));
        }
    }
}
