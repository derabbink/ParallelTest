using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Test.Chaining
{
    public class ChainingTest
    {
        protected IExecutor _executor;
        protected ManualResetEventSlim _notify;
        protected ManualResetEventSlim _hold;
        protected int _timeout;
        protected Exception _argument1;
        protected Tuple<Exception, Exception> _argument2;
        protected Tuple<Exception, Exception, Exception> _argument3;
        protected Tuple<Exception, Exception, Exception, Exception> _argument4;
        protected Tuple<Exception, Exception, Exception, Exception, Exception> _argument5;
        protected Tuple<Exception, Exception, Exception, Exception, Exception, Exception> _argument6;
        protected Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception> _argument7;

        #region 1_1
        protected Func<CancellationToken, Action, Exception, Exception> _1_1;
        protected Func<CancellationToken, Action, Exception, Exception> _blocking1_1;
        protected Func<CancellationToken, Action, Exception, Exception> _throw1_1;
        protected CoordinatedInstruction<Exception, Exception> _1_1Instr;
        protected CoordinatedInstruction<Exception, Exception> _blocking1_1Instr;
        protected CoordinatedInstruction<Exception, Exception> _timeoutable1_1Instr;
        protected CoordinatedInstruction<Exception, Exception> _throw1_1Instr;
        #endregion
        #region 2_2
        protected Func<CancellationToken, Action, Tuple<Exception, Exception>, Tuple<Exception, Exception>> _2_2;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception>, Tuple<Exception, Exception>> _blocking2_2;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception>, Tuple<Exception, Exception>> _throw2_2;
        protected CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>> _2_2Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>> _blocking2_2Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>> _timeoutable2_2Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>> _throw2_2Instr;
        #endregion
        #region 3_3
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _3_3;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _blocking3_3;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _throw3_3;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _3_3Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _blocking3_3Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _timeoutable3_3Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>> _throw3_3Instr;
        #endregion
        #region 4_4
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _4_4;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _blocking4_4;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _throw4_4;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _4_4Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _blocking4_4Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _timeoutable4_4Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>> _throw4_4Instr;
        #endregion
        #region 5_5
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _5_5;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _blocking5_5;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _throw5_5;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _5_5Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _blocking5_5Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _timeoutable5_5Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>> _throw5_5Instr;
        #endregion
        #region 6_6
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _6_6;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _blocking6_6;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _throw6_6;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _6_6Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _blocking6_6Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _timeoutable6_6Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>> _throw6_6Instr;
        #endregion
        #region 7_7
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _7_7;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _blocking7_7;
        protected Func<CancellationToken, Action, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _throw7_7;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _7_7Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _blocking7_7Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _timeoutable7_7Instr;
        protected CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>> _throw7_7Instr;
        #endregion

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _notify = new ManualResetEventSlim(false);
            _hold = new ManualResetEventSlim(false);
            _timeout = -1;
            
            Setup1_();
            Setup2_();
            Setup3_();
            Setup4_();
            Setup5_();
            Setup6_();
            Setup7_();
        }

        #region 1_
        private void Setup1_()
        {
            _argument1 = new Exception("Expected");
            Setup1_1();
        }
        private void Setup1_1()
        {
            _1_1 = (_, p, e) => e;
            _blocking1_1 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw1_1 = (_, p, e) => { throw e; };
            _1_1Instr = new CoordinatedInstruction<Exception, Exception>(_executor, _1_1, _timeout);
            _blocking1_1Instr = new CoordinatedInstruction<Exception, Exception>(_executor, _blocking1_1, _timeout);
            _timeoutable1_1Instr = new CoordinatedInstruction<Exception, Exception>(_executor, _blocking1_1);
            _throw1_1Instr = new CoordinatedInstruction<Exception, Exception>(_executor, _throw1_1, _timeout);
        }
        #endregion

        #region 2_
        private void Setup2_()
        {
            _argument2 = new Tuple<Exception, Exception>(_argument1, _argument1);
            Setup2_2();
        }
        private void Setup2_2()
        {
            _2_2 = (_, p, e) => e;
            _blocking2_2 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw2_2 = (_, p, e) => { throw e.Item1; };
            _2_2Instr = new CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>>(_executor, _2_2, _timeout);
            _blocking2_2Instr = new CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>>(_executor, _blocking2_2, _timeout);
            _timeoutable2_2Instr = new CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>>(_executor, _blocking2_2);
            _throw2_2Instr = new CoordinatedInstruction<Tuple<Exception, Exception>, Tuple<Exception, Exception>>(_executor, _throw2_2, _timeout);
        }
        #endregion

        #region 3_
        private void Setup3_()
        {
            _argument3 = new Tuple<Exception, Exception, Exception>(_argument1, _argument1, _argument1);
            Setup3_3();
        }
        private void Setup3_3()
        {
            _3_3 = (_, p, e) => e;
            _blocking3_3 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw3_3 = (_, p, e) => { throw e.Item1; };
            _3_3Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>>(_executor, _3_3, _timeout);
            _blocking3_3Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>>(_executor, _blocking3_3, _timeout);
            _timeoutable3_3Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>>(_executor, _blocking3_3);
            _throw3_3Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception>, Tuple<Exception, Exception, Exception>>(_executor, _throw3_3, _timeout);
        }
        #endregion

        #region 4_
        private void Setup4_()
        {
            _argument4 = new Tuple<Exception, Exception, Exception, Exception>(_argument1, _argument1, _argument1, _argument1);
            Setup4_4();
        }
        private void Setup4_4()
        {
            _4_4 = (_, p, e) => e;
            _blocking4_4 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw4_4 = (_, p, e) => { throw e.Item1; };
            _4_4Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>>(_executor, _4_4, _timeout);
            _blocking4_4Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>>(_executor, _blocking4_4, _timeout);
            _timeoutable4_4Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>>(_executor, _blocking4_4);
            _throw4_4Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception>>(_executor, _throw4_4, _timeout);
        }
        #endregion

        #region 5_
        private void Setup5_()
        {
            _argument5 = new Tuple<Exception, Exception, Exception, Exception, Exception>(_argument1, _argument1, _argument1, _argument1, _argument1);
            Setup5_5();
        }
        private void Setup5_5()
        {
            _5_5 = (_, p, e) => e;
            _blocking5_5 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw5_5 = (_, p, e) => { throw e.Item1; };
            _5_5Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>>(_executor, _5_5, _timeout);
            _blocking5_5Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking5_5, _timeout);
            _timeoutable5_5Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking5_5);
            _throw5_5Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception>>(_executor, _throw5_5, _timeout);
        }
        #endregion

        #region 6_
        private void Setup6_()
        {
            _argument6 = new Tuple<Exception, Exception, Exception, Exception, Exception, Exception>(_argument1, _argument1, _argument1, _argument1, _argument1, _argument1);
            Setup6_6();
        }
        private void Setup6_6()
        {
            _6_6 = (_, p, e) => e;
            _blocking6_6 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw6_6 = (_, p, e) => { throw e.Item1; };
            _6_6Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _6_6, _timeout);
            _blocking6_6Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking6_6, _timeout);
            _timeoutable6_6Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking6_6);
            _throw6_6Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _throw6_6, _timeout);
        }
        #endregion

        #region 7_
        private void Setup7_()
        {
            _argument7 = new Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(_argument1, _argument1, _argument1, _argument1, _argument1, _argument1, _argument1);
            Setup7_7();
        }
        private void Setup7_7()
        {
            _7_7 = (_, p, e) => e;
            _blocking7_7 = (ct, p, a) => { _notify.Set(); _hold.Wait(ct); return a; };
            _throw7_7 = (_, p, e) => { throw e.Item1; };
            _7_7Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _7_7, _timeout);
            _blocking7_7Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking7_7, _timeout);
            _timeoutable7_7Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _blocking7_7);
            _throw7_7Instr = new CoordinatedInstruction<Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>, Tuple<Exception, Exception, Exception, Exception, Exception, Exception, Exception>>(_executor, _throw7_7, _timeout);
        }
        #endregion
        #endregion
    }
}
