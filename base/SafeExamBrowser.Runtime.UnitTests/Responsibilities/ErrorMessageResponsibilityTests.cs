/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class ErrorMessageResponsibilityTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private Mock<IMailClient> mailClient;
		private Mock<IMessageBox> messageBox;
		private RuntimeContext context;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ErrorMessageResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig
			{
				BrowserLogFilePath = @"C:\Logs\Browser.log",
				ClientLogFilePath = @"C:\Logs\Client.log",
				RuntimeLogFilePath = @"C:\Logs\Runtime.log",
				ServiceLogFilePath = @"C:\Logs\Service.log"
			};
			logger = new Mock<ILogger>();
			mailClient = new Mock<IMailClient>();
			messageBox = new Mock<IMessageBox>();
			context = new RuntimeContext();
			runtimeWindow = new Mock<IRuntimeWindow>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns<TextKey>(key => key.ToString());

			sut = new ErrorMessageResponsibility(appConfig, logger.Object, mailClient.Object, context, runtimeWindow.Object, splashScreen.Object, text.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustShowMessageBoxForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains(TextKey.ErrorDialog_StartupMessage.ToString())),
				It.Is<string>(title => title == TextKey.ErrorDialog_StartupTitle.ToString()),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(icon => icon == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustShowMessageBoxForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains(TextKey.ErrorDialog_ShutdownMessage.ToString())),
				It.Is<string>(title => title == TextKey.ErrorDialog_ShutdownTitle.ToString()),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(icon => icon == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustUseSplashScreenAsParentForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(parent => parent == splashScreen.Object)), Times.Once);
		}

		[TestMethod]
		public void MustUseSplashScreenAsParentForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(parent => parent == splashScreen.Object)), Times.Once);
		}

		[TestMethod]
		public void MustRetrieveLocalizedTextForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			text.Verify(t => t.Get(TextKey.ErrorDialog_StartupMessage), Times.Once);
			text.Verify(t => t.Get(TextKey.ErrorDialog_StartupTitle), Times.Once);
		}

		[TestMethod]
		public void MustRetrieveLocalizedTextForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			text.Verify(t => t.Get(TextKey.ErrorDialog_ShutdownMessage), Times.Once);
			text.Verify(t => t.Get(TextKey.ErrorDialog_ShutdownTitle), Times.Once);
		}

		[TestMethod]
		public void MustIgnoreUnrelatedTasks()
		{
			foreach (var task in (RuntimeTask[]) Enum.GetValues(typeof(RuntimeTask)))
			{
				if (task != RuntimeTask.ShowStartupError && task != RuntimeTask.ShowShutdownError)
				{
					sut.Assume(task);
				}
			}

			messageBox.VerifyNoOtherCalls();
		}
	}
}