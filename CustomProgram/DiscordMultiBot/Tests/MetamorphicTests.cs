using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Discord;
using Discord.WebSocket;
using Moq;

namespace DiscordMultiBot.Tests
{
    public class MetamorphicTests
    {
        private readonly Mock<DiscordSocketClient> _mockClient;
        private readonly Bot _bot;

        public MetamorphicTests()
        {
            _mockClient = new Mock<DiscordSocketClient>();
            _bot = new Bot(_mockClient.Object);
        }

        private async Task SetupPlaylist(List<string> playlist)
        {
            await _bot.ClearPlaylist();
            foreach (var song in playlist)
            {
                await _bot.AddToPlaylist(song);
            }
        }

        [Fact]
        public async Task AddRemoveSong()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3" };
            await SetupPlaylist(originalPlaylist);

            //Add a song, then remove it to playlist
            await _bot.AddToPlaylist("NewSong");
            await _bot.RemoveFromPlaylist("NewSong");

            var finalPlaylist1 = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist, finalPlaylist1);

            //Remove a song, then add it back to playlist
            await _bot.RemoveFromPlaylist("Song2");
            await _bot.AddToPlaylist("Song2");

            var finalPlaylist2 = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist, finalPlaylist2);
        }

        [Fact]
        public async Task ShuffleUnshuffle()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3", "Song4", "Song5" };
            await SetupPlaylist(originalPlaylist);

            // Shuffle the playlist
            await _bot.ShufflePlaylist();
            var shuffledPlaylist = await _bot.GetPlaylist();
            Assert.NotEqual(originalPlaylist, shuffledPlaylist);

            // Unshuffle the playlist
            await _bot.UnshufflePlaylist();
            var unshuffledPlaylist = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist, unshuffledPlaylist);
        }

        [Fact]
        public async Task MoveSongs()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3", "Song4" };
            await SetupPlaylist(originalPlaylist);

            // Move songs around
            await _bot.MoveSong("Song1", 3);
            await _bot.MoveSong("Song4", 0);

            var finalPlaylist = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist.Count, finalPlaylist.Count);
            Assert.All(originalPlaylist, song => Assert.Contains(song, finalPlaylist));
        }

        [Fact]
        public async Task DuplicateSongs()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3" };
            await SetupPlaylist(originalPlaylist);

            // Add duplicate entries
            await _bot.AddToPlaylist("Song2");
            await _bot.AddToPlaylist("Song2");

            var playlistWithDuplicates = await _bot.GetPlaylist();
            Assert.Equal(5, playlistWithDuplicates.Count);
            Assert.Equal(3, playlistWithDuplicates.Count(s => s == "Song2"));

            // Remove all instances of the duplicated song
            await _bot.RemoveAllInstancesOfSong("Song2");

            var finalPlaylist = await _bot.GetPlaylist();
            Assert.Equal(2, finalPlaylist.Count);
            Assert.DoesNotContain("Song2", finalPlaylist);
        }

        [Fact]
        public async Task ClearAndRebuildPlaylist()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3" };
            await SetupPlaylist(originalPlaylist);

            // Clear the playlist
            await _bot.ClearPlaylist();
            var emptyPlaylist = await _bot.GetPlaylist();
            Assert.Empty(emptyPlaylist);

            // Rebuild the playlist
            foreach (var song in originalPlaylist)
            {
                await _bot.AddToPlaylist(song);
            }

            var rebuiltPlaylist = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist, rebuiltPlaylist);
        }

        [Fact]
        public async Task ReversePlaylistTwice()
        {
            var originalPlaylist = new List<string> { "Song1", "Song2", "Song3", "Song4", "Song5" };
            await SetupPlaylist(originalPlaylist);

            // Reverse the playlist twice
            await _bot.ReversePlaylist();
            await _bot.ReversePlaylist();

            var finalPlaylist = await _bot.GetPlaylist();
            Assert.Equal(originalPlaylist, finalPlaylist);
        }

        [Fact]
        public async Task RoleBasedCommandAccess()
        {
            var mockUser = new Mock<IGuildUser>();
            var mockRole = new Mock<IRole>();
            mockRole.Setup(r => r.Permissions).Returns(new GuildPermissions(manageChannels: true));
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions(manageChannels: true));

            // User with role should have access
            var hasAccess1 = await _bot.UserHasPermission(mockUser.Object, "manage_channels");
            Assert.True(hasAccess1);

            // Remove role, user should lose access
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions(manageChannels: false));
            var hasAccess2 = await _bot.UserHasPermission(mockUser.Object, "manage_channels");
            Assert.False(hasAccess2);

            // Add role back, user should regain access
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions(manageChannels: true));
            var hasAccess3 = await _bot.UserHasPermission(mockUser.Object, "manage_channels");
            Assert.True(hasAccess3);
        }

        [Fact]
        public async Task ChannelSpecificPermissions()
        {
            var mockUser = new Mock<IGuildUser>();
            var mockChannel = new Mock<IGuildChannel>();
            
            // Set up global permissions
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions(manageChannels: false));

            // Set up channel-specific permissions
            var channelPerms = new OverwritePermissions(manageChannel: PermValue.Allow);
            mockChannel.Setup(c => c.GetPermissionOverwrite(It.IsAny<IGuildUser>())).Returns(channelPerms);

            var hasAccess = await _bot.UserHasChannelPermission(mockUser.Object, mockChannel.Object, "manage_channel");
            Assert.True(hasAccess);
        }

        [Fact]
        public async Task TimeBasedAccessRestrictions()
        {
            var mockUser = new Mock<IGuildUser>();
            var restrictedTime = new TimeSpan(22, 0, 0); // 10 PM
            var allowedTime = new TimeSpan(14, 0, 0); // 2 PM

            // User should not have access during restricted time
            var hasAccess1 = await _bot.UserHasTimeBasedAccess(mockUser.Object, restrictedTime);
            Assert.False(hasAccess1);

            // User should have access during allowed time
            var hasAccess2 = await _bot.UserHasTimeBasedAccess(mockUser.Object, allowedTime);
            Assert.True(hasAccess2);
        }

        [Fact]
        public async Task UserMuteUnmute()
        {
            var mockUser = new Mock<IGuildUser>();
            
            // Initially, user is not muted
            mockUser.Setup(u => u.IsMuted).Returns(false);
            var canSpeak1 = await _bot.UserCanSpeak(mockUser.Object);
            Assert.True(canSpeak1);

            // Mute the user
            mockUser.Setup(u => u.IsMuted).Returns(true);
            var canSpeak2 = await _bot.UserCanSpeak(mockUser.Object);
            Assert.False(canSpeak2);

            // Unmute the user
            mockUser.Setup(u => u.IsMuted).Returns(false);
            var canSpeak3 = await _bot.UserCanSpeak(mockUser.Object);
            Assert.True(canSpeak3);
        }

        [Fact]
        public async Task BotOwnerOverride()
        {
            var mockUser = new Mock<IGuildUser>();
            var mockChannel = new Mock<IGuildChannel>();
            
            // Set up user as not having any permissions
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions());
            mockChannel.Setup(c => c.GetPermissionOverwrite(It.IsAny<IGuildUser>())).Returns((OverwritePermissions?)null);

            // Normal user should not have access
            var hasAccess1 = await _bot.UserHasPermission(mockUser.Object, "administrator");
            Assert.False(hasAccess1);

            // Set user as bot owner
            _bot.SetBotOwner(mockUser.Object.Id);

            // Bot owner should have access to everything
            var hasAccess2 = await _bot.UserHasPermission(mockUser.Object, "administrator");
            Assert.True(hasAccess2);
        }

        [Fact]
        public async Task TemporaryRole()
        {
            var mockUser = new Mock<IGuildUser>();
            var mockRole = new Mock<IRole>();
            mockRole.Setup(r => r.Permissions).Returns(new GuildPermissions(manageMessages: true));
            
            // Initially, user doesn't have the role
            mockUser.Setup(u => u.GuildPermissions).Returns(new GuildPermissions(manageMessages: false));
            var initialAccess = await _bot.UserHasPermission(mockUser.Object, "manage_messages");
            Assert.False(initialAccess);

            // Temporarily assign the role
            await _bot.AssignTemporaryRole(mockUser.Object, mockRole.Object, TimeSpan.FromMinutes(5));
            var temporaryAccess = await _bot.UserHasPermission(mockUser.Object, "manage_messages");
            Assert.True(temporaryAccess);

            // After the temporary period, the access should be revoked
            await Task.Delay(TimeSpan.FromMinutes(5));
            var finalAccess = await _bot.UserHasPermission(mockUser.Object, "manage_messages");
            Assert.False(finalAccess);
        }

    }
}