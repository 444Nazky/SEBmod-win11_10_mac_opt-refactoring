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
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class SessionOperationSequenceTests
	{
		private Mock<ILogger> logger;
		private Mock<SessionOperation> operationA;
		private Mock<SessionOperation> operationB;
		private Mock<IRuntimeWindow> runtimeWindow;

		private SessionOperationSequence sut;

		[TestInitialize]
		public void Initialize()
		{
			var runtimeContext = new RuntimeContext();
			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), runtimeContext),
				Mock.Of<ILogger>(),
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				runtimeContext,
				Mock.Of<IText>());

			logger = new Mock<ILogger>();
			operationA = new Mock<SessionOperation>(dependencies);
			operationB = new Mock<SessionOperation>(dependencies);
			runtimeWindow = new Mock<IRuntimeWindow>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Repeat()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Revert()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Repeat()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Revert()).Returns(OperationResult.Success);

			sut = new SessionOperationSequence(logger.Object, new[] { operationA.Object, operationB.Object }, runtimeWindow.Object);
		}

		[TestMethod]
		public void MustUpdateStatusOfRuntimeWindowWhenOperationFiresStatusChanged()
		{
			var status = TextKey.OperationStatus_VerifySessionIntegrity;

			operationA.Raise(o => o.StatusChanged += null, status);

			runtimeWindow.Verify(w => w.UpdateStatus(status, true), Times.Once);
		}

		[TestMethod]
		public void MustUpdateStatusOfRuntimeWindowForEveryOperation()
		{
			var statusA = TextKey.OperationStatus_VerifySessionIntegrity;
			var statusB = TextKey.OperationStatus_InitializeSession;

			operationA.Raise(o => o.StatusChanged += null, statusA);
			operationB.Raise(o => o.StatusChanged += null, statusB);

			runtimeWindow.Verify(w => w.UpdateStatus(statusA, true), Times.Once);
			runtimeWindow.Verify(w => w.UpdateStatus(statusB, true), Times.Once);
		}

		[TestMethod]
		public void TryPerform_MustInitializeProgressOfRuntimeWindow()
		{
			var result = sut.TryPerform();

			runtimeWindow.Verify(w => w.SetValue(0), Times.Once);
			runtimeWindow.Verify(w => w.SetMaxValue(2), Times.Once);
			runtimeWindow.Verify(w => w.SetIndeterminate(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void TryPerform_MustReportProgressForEachOperation()
		{
			var result = sut.TryPerform();

			operationA.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			runtimeWindow.Verify(w => w.Progress(), Times.Exactly(2));
			runtimeWindow.Verify(w => w.Regress(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void TryPerform_MustReportRegressWhenOperationFails()
		{
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Failed);

			var result = sut.TryPerform();

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			runtimeWindow.Verify(w => w.Progress(), Times.Once);
			runtimeWindow.Verify(w => w.Regress(), Times.Exactly(2));

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void TryRepeat_MustInitializeProgressOfRuntimeWindow()
		{
			var result = sut.TryRepeat();

			runtimeWindow.Verify(w => w.SetValue(0), Times.Once);
			runtimeWindow.Verify(w => w.SetMaxValue(2), Times.Once);
			runtimeWindow.Verify(w => w.SetIndeterminate(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void TryRepeat_MustReportProgressForEachOperation()
		{
			var result = sut.TryRepeat();

			operationA.Verify(o => o.Repeat(), Times.Once);
			operationB.Verify(o => o.Repeat(), Times.Once);
			runtimeWindow.Verify(w => w.Progress(), Times.Exactly(2));

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void TryRepeat_MustNotReportProgressForFailedOperation()
		{
			operationA.Setup(o => o.Repeat()).Returns(OperationResult.Failed);

			var result = sut.TryRepeat();

			operationB.Verify(o => o.Repeat(), Times.Never);
			runtimeWindow.Verify(w => w.Progress(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void TryRevert_MustSetProgressOfRuntimeWindowToIndeterminate()
		{
			var result = sut.TryRevert();

			runtimeWindow.Verify(w => w.SetIndeterminate(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void TryRevert_MustNotReportRegress()
		{
			sut.TryPerform();
			runtimeWindow.Invocations.Clear();

			var result = sut.TryRevert();

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			runtimeWindow.Verify(w => w.Regress(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustNotFailWithoutRuntimeWindow()
		{
			var sequence = new SessionOperationSequence(logger.Object, new[] { operationA.Object, operationB.Object }, default);
			var perform = sequence.TryPerform();
			var repeat = sequence.TryRepeat();

			operationA.Raise(o => o.StatusChanged += null, TextKey.OperationStatus_VerifySessionIntegrity);

			var revert = sequence.TryRevert();

			Assert.AreEqual(OperationResult.Success, perform);
			Assert.AreEqual(OperationResult.Success, repeat);
			Assert.AreEqual(OperationResult.Success, revert);
		}

		[TestMethod]
		public void MustNotFailWithoutRuntimeWindowWhenOperationFails()
		{
			var sequence = new SessionOperationSequence(logger.Object, new[] { operationA.Object, operationB.Object }, default);

			operationB.Setup(o => o.Perform()).Returns(OperationResult.Failed);

			var result = sequence.TryPerform();

			Assert.AreEqual(OperationResult.Failed, result);
		}
	}
}