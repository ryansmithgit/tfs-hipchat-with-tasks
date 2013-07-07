﻿using System;
using System.Collections.Generic;
using NSubstitute;
using TfsHipChat.Tfs.Events;
using Xunit;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace TfsHipChat.Tests
{
    public class TfsHipChatEventServiceTests
    {
        private const string TfsIdentityXml = "";

        [Fact]
        public void Notify_ShouldThrowException_WhenInvalidXmlData()
        {
            var notificationHandler = Substitute.For<INotificationHandler>();
            var eventService = new TfsHipChatEventService(notificationHandler);
            const string eventXml = "invalid_xml";

            Assert.Throws<XmlException>(() => eventService.Notify(eventXml, TfsIdentityXml));
        }

        [Fact]
        public void Notify_ShouldThrowException_WhenUnsupportedEvent()
        {
            var notificationHandler = Substitute.For<INotificationHandler>();
            var eventService = new TfsHipChatEventService(notificationHandler);
            const string eventXml = "<EventThatDoesNotExist></EventThatDoesNotExist>";

            Assert.Throws<NotSupportedException>(() => eventService.Notify(eventXml, TfsIdentityXml));
        }

        [Fact]
        public void Notify_ShouldHandleCheckinEvent()
        {
            var notificationHandler = Substitute.For<INotificationHandler>();
            var eventService = new TfsHipChatEventService(notificationHandler);
            var eventXml = CreateSerializedEvent<CheckinEvent>();

            eventService.Notify(eventXml, TfsIdentityXml);

            notificationHandler.ReceivedWithAnyArgs().HandleCheckin(null);
        }

        [Fact]
        public void Notify_ShouldHandleBuildCompletionEvent()
        {
            var notificationHandler = Substitute.For<INotificationHandler>();
            var eventService = new TfsHipChatEventService(notificationHandler);
            var eventXml = CreateSerializedEvent<BuildCompletionEvent>();

            eventService.Notify(eventXml, TfsIdentityXml);

            notificationHandler.ReceivedWithAnyArgs().HandleBuildCompletion(null);
        }

        [Fact]
        public void Notify_ShouldHandleBuildCompletedEvent()
        {
            var notificationHandler = Substitute.For<INotificationHandler>();
            var eventService = new TfsHipChatEventService(notificationHandler);
            var eventXml = CreateSerializedEvent(new BuildCompletedEvent
                                 {
                                     Build = new BuildDetail(),
                                     Controller = new BuildController(),
                                     Requests = new List<QueuedBuild>(new[] {new QueuedBuild()})
                                 });

            eventService.Notify(eventXml, TfsIdentityXml);

            notificationHandler.ReceivedWithAnyArgs().HandleBuildCompletion(null);
        }

        private static string CreateSerializedEvent<T>() where T : new()
        {
            var eventObject = new T();
            return CreateSerializedEvent(eventObject);
        }

        private static string CreateSerializedEvent<T>(T eventObject) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));
            var sw = new StringWriter();
            serializer.Serialize(sw, eventObject);
            return sw.ToString();
        }
    }
}
