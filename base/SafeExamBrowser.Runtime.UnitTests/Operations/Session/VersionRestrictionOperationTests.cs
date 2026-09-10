/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class VersionRestrictionOperationTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private RuntimeContext runtimeContext;
		private Mock<IText> text;

		private VersionRestrictionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			runtimeContext = new RuntimeContext();
			text = new Mock<IText>();

			appConfig.ProgramBuildVersion = "3.7.1.512";
			appConfig.ProgramInformationalVersion = "SEB 3.7.1";
			nextSession.AppConfig = appConfig;
			nextSession.Settings = nextSettings;
			runtimeContext.Current = default;
			runtimeContext.Next = nextSession;

			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns(string.Empty);

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), runtimeContext),
				logger.Object,
				messageBox.Object,
				Mock.Of<IRuntimeWindow>(),
				runtimeContext,
				text.Object);

			sut = new VersionRestrictionOperation(dependencies);
		}

		[TestMethod]
		public void Perform_MustSucceedWithoutRestrictions()
		{
			var result = sut.Perform();

			messageBox.VerifyNoOtherCalls();
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Perform();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_ValidateVersionRestrictions, key);
		}

		[TestMethod]
		public void Perform_MustSucceedWithMatchingExactRestriction()
		{
			AddRestriction(major: 3, minor: 7);

			var result = sut.Perform();

			messageBox.VerifyNoOtherCalls();
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortWithMismatchingExactRestriction()
		{
			AddRestriction(major: 3, minor: 6);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustValidatePatchOfExactRestriction()
		{
			AddRestriction(major: 3, minor: 7, patch: 1);
			Assert.AreEqual(OperationResult.Success, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 3, minor: 7, patch: 2);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustValidateBuildOfExactRestriction()
		{
			AddRestriction(major: 3, minor: 7, patch: 1, build: 512);
			Assert.AreEqual(OperationResult.Success, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 3, minor: 7, patch: 1, build: 513);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustValidateAllianceEditionOfExactRestriction()
		{
			AddRestriction(major: 3, minor: 7, requiresAllianceEdition: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());

			appConfig.ProgramInformationalVersion = "SEB 3.7.1 Alliance Edition";
			Assert.AreEqual(OperationResult.Success, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustSucceedWithLowerMinimumRestriction()
		{
			AddRestriction(major: 3, minor: 5, isMinimum: true);
			Assert.AreEqual(OperationResult.Success, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 2, minor: 9, isMinimum: true);
			Assert.AreEqual(OperationResult.Success, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustSucceedWithEqualMinimumRestriction()
		{
			AddRestriction(major: 3, minor: 7, patch: 1, build: 512, isMinimum: true);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortWithHigherMinimumRestriction()
		{
			AddRestriction(major: 4, minor: 0, isMinimum: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 3, minor: 8, isMinimum: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 3, minor: 7, patch: 2, isMinimum: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());

			ClearRestrictions();

			AddRestriction(major: 3, minor: 7, patch: 1, build: 600, isMinimum: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustIgnorePatchOfMinimumRestrictionWithLowerMinor()
		{
			AddRestriction(major: 3, minor: 6, patch: 99, build: 9999, isMinimum: true);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustValidateAllianceEditionOfMinimumRestriction()
		{
			AddRestriction(major: 3, minor: 5, isMinimum: true, requiresAllianceEdition: true);
			Assert.AreEqual(OperationResult.Aborted, sut.Perform());

			appConfig.ProgramInformationalVersion = "SEB 3.7.1 Alliance Edition";
			Assert.AreEqual(OperationResult.Success, sut.Perform());
		}

		[TestMethod]
		public void Perform_MustSucceedIfAnyRestrictionIsFulfilled()
		{
			AddRestriction(major: 2, minor: 0);
			AddRestriction(major: 3, minor: 7);

			var result = sut.Perform();

			messageBox.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortIfNoRestrictionIsFulfilled()
		{
			AddRestriction(major: 2, minor: 0);
			AddRestriction(major: 4, minor: 0, isMinimum: true);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustShowErrorMessageWhenAborting()
		{
			text.Setup(t => t.Get(TextKey.MessageBox_VersionRestrictionError)).Returns("%%_VERSION_%%");
			AddRestriction(major: 3, minor: 6);

			var result = sut.Perform();

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains("SEB 3.7.1")),
				It.IsAny<string>(),
				MessageBoxAction.Ok,
				MessageBoxIcon.Error,
				It.IsAny<IWindow>()), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustListRequiredVersionsInErrorMessage()
		{
			AddRestriction(major: 4, minor: 1, patch: 2, requiresAllianceEdition: true);
			text.Setup(t => t.Get(TextKey.MessageBox_VersionRestrictionError)).Returns("%%_REQUIRED_VERSIONS_%%");

			sut.Perform();

			messageBox.Verify(m => m.Show(
				It.Is<string>(message => message.Contains("SEB 4.1.2 Alliance Edition")),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotShowErrorMessageWhenSucceeding()
		{
			AddRestriction(major: 3, minor: 7);

			sut.Perform();

			messageBox.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Repeat_MustValidateRestrictionsLikePerform()
		{
			AddRestriction(major: 3, minor: 7);
			Assert.AreEqual(OperationResult.Success, sut.Repeat());

			ClearRestrictions();
			AddRestriction(major: 3, minor: 6);

			var result = sut.Repeat();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Repeat();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_ValidateVersionRestrictions, key);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var fired = false;

			AddRestriction(major: 2, minor: 0);
			sut.StatusChanged += (_) => fired = true;

			var result = sut.Revert();

			messageBox.VerifyNoOtherCalls();
			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsFalse(fired);
		}

		private void AddRestriction(int major, int minor, int? patch = default, int? build = default, bool isMinimum = false, bool requiresAllianceEdition = false)
		{
			nextSettings.Security.VersionRestrictions.Add(new VersionRestriction
			{
				Major = major,
				Minor = minor,
				Patch = patch,
				Build = build,
				IsMinimumRestriction = isMinimum,
				RequiresAllianceEdition = requiresAllianceEdition
			});
		}

		private void ClearRestrictions()
		{
			nextSettings.Security.VersionRestrictions.Clear();
		}
	}
}