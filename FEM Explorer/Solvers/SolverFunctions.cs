using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using MathNet.Numerics.LinearAlgebra;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI;





namespace FEM_Explorer
{
    internal static class SolverFunctions
    {
        private static bool HasErrors = false;
        private static bool HasWarning = false;

        private static Stopwatch MainTimer = new Stopwatch();
        private static long StageStart;


        private static int UnrestrainedDOF = 0;
        private static int RestrainedDOF = 0;
        private static int TotalDOF = 0;


        private static Matrix<double> K;
        //private Matrix<double> K11square;
        private static Matrix<double> K11;
        private static Matrix<double> K12;
        private static Matrix<double> K21;
        private static Matrix<double> K22;

        private static Matrix<double> Qk;
        private static Matrix<double> Qu;
        private static Matrix<double> Dk;
        private static Matrix<double> Du;

        private static double[][] DK;
        private static double[][] DK11;
        //private static  double[][] DK11Inversed;
        private static double[][] DK21;

        private static double[] DQk;
        private static double[] DQu;
        //private static  double[] DDk;
        private static double[] DDu;


        #region Thread Communications

        public static async void AddMessage(long total, long step, string message, int messageType)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        SolverDisplay.Current.AddMessage(total, step, message, messageType);
                    }
                    );
        }

        #endregion

        #region Preperation

        internal static async void ResetDisplayAndShowWelcomeMessage()
        {
            MainTimer.Start();
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        SolverDisplay.Current.ClearMessages();
                    }
                    );

            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Starting solver DoubleLUP.", 0);
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "    Local matrices for elements and nodes have been created during construction stage.", 0);
        }

        internal static void ResetPropertiesForSolver()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Reset solver properties.", 0);

            try
            {
                Model.Members.CurrentMember = null;
                Camera.LargestLengthRatio = 0;
                Camera.LargestAxialRatio = 0;
                HasErrors = false;


                UnrestrainedDOF = 0;
                RestrainedDOF = 0;
                TotalDOF = 0;

                K = null;
                K11 = null;
                K12 = null;
                K21 = null;
                K22 = null;

                Qk = null;
                Qu = null;
                Dk = null;
                Du = null;

                DK = null;
                DK11 = null;
                DK21 = null;
                DQk = null;
                DQu = null;
                DDu = null;

                Model.ForceX = 0;
                Model.ForceY = 0;
                Model.ForceM = 0;
                Model.ReactionX = 0;
                Model.ReactionY = 0;
                Model.ReactionM = 0;

                Model.TotalCost = 0;
                Model.MaterialCost = 0;
                Model.NodeCost = 0;
                Model.TransportCost = 0;
                Model.ElevationCost = 0;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);

        }

        internal static void DisplayModelProperties()
        {
            AddMessage(-1, -1, "Model Properties.", 0);
            AddMessage(-1, -1, "    " + Model.Members.Count.ToString("#,###") + " Members.", 1);
            AddMessage(-1, -1, "    " + Model.Members.MembersWithLinearLoads.Count.ToString("#,###") + " Members with linear loads.", 1);
            AddMessage(-1, -1, "    " + Model.Nodes.Count.ToString("#,###") + " Nodes.", 1);
            AddMessage(-1, -1, "    " + Model.Nodes.NodesWithConstraints.Count.ToString("#,###") + " nodes with constraints.", 1);
            AddMessage(-1, -1, "    " + Model.Nodes.NodesWithNodalLoads.Count.ToString("#,###") + " nodes with nodal loads.", 1);

        }

        internal static void ShrinkModel()
        {
            StageStart = MainTimer.ElapsedMilliseconds;
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Shrinking Model. (searching for orphan nodes)", 0);

            int PreviousNodeCount = 0;
            int PostNodeCount = 0;

            try
            {
                PreviousNodeCount = Model.Nodes.Count;
                Model.Shrink();
                PostNodeCount = Model.Nodes.Count;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, MainTimer.ElapsedMilliseconds - StageStart, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, MainTimer.ElapsedMilliseconds - StageStart, "    " + ex.Message, 2);
            }
            
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "    Started with " + PreviousNodeCount + " nodes.", 1);
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "    Ended with " + PostNodeCount + " nodes.", 1);
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "    Removed " + (PreviousNodeCount - PostNodeCount) + " nodes.", 1);

            AddMessage(MainTimer.ElapsedMilliseconds, MainTimer.ElapsedMilliseconds - StageStart, "    Finished.", 1);
        }

        internal static async void SaveFile()
        {
            StageStart = MainTimer.ElapsedMilliseconds;
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Saving File.", 0);
            await FileManager.SaveFile();
            AddMessage(MainTimer.ElapsedMilliseconds, MainTimer.ElapsedMilliseconds - StageStart, "    Finished.", 1);
        }

        #endregion

        internal static void AssignCodeNumbersToDegreesOfFreedom()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Assigning code numbers to the degrees of freedom.", 0);
            AddMessage(-1, -1, "    Each node has three degrees of freedom.", 1);

            try
            {
                //Assign code numbers to the unrestrained first.          
                foreach (var Item in Model.Nodes)
                {
                    if (Item.Value.Constraints.X == false)
                    {
                        Item.Value.Codes = new Codes(UnrestrainedDOF, Item.Value.Codes.Y, Item.Value.Codes.M);
                        UnrestrainedDOF++;
                    }
                    if (Item.Value.Constraints.Y == false)
                    {
                        Item.Value.Codes = new Codes(Item.Value.Codes.X, UnrestrainedDOF, Item.Value.Codes.M);
                        UnrestrainedDOF++;
                    }
                    if (Item.Value.Constraints.M == false)
                    {
                        Item.Value.Codes = new Codes(Item.Value.Codes.X, Item.Value.Codes.Y, UnrestrainedDOF);
                        UnrestrainedDOF++;
                    }
                }

                //Then assign code number to the remaining.
                foreach (var Item in Model.Nodes)
                {
                    if (Item.Value.Constraints.X == true)
                    {
                        Item.Value.Codes = new Codes(UnrestrainedDOF + RestrainedDOF, Item.Value.Codes.Y, Item.Value.Codes.M);
                        RestrainedDOF++;
                    }
                    if (Item.Value.Constraints.Y == true)
                    {
                        Item.Value.Codes = new Codes(Item.Value.Codes.X, UnrestrainedDOF + RestrainedDOF, Item.Value.Codes.M);
                        RestrainedDOF++;
                    }
                    if (Item.Value.Constraints.M == true)
                    {
                        Item.Value.Codes = new Codes(Item.Value.Codes.X, Item.Value.Codes.Y, UnrestrainedDOF + RestrainedDOF);
                        RestrainedDOF++;
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }

            AddMessage(-1, -1, "    Total unrestrained degrees of freedom " + UnrestrainedDOF + ".", 1);
            AddMessage(-1, -1, "    Total restrained degrees of freedom " + RestrainedDOF + ".", 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void CreateSuperpositionValuesFromSegmentsToNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Superposition Stage One.", 0);

            try
            {
                foreach (var NodeItem in Model.Nodes)
                {
                    NodeItem.Value.SuperPosition = new FEM_Explorer.NodalLoad(0, 0, 0);
                    NodeItem.Value.SuperPosition = new FEM_Explorer.NodalLoad(0, 0, 0);
                }
                foreach (var Item in Model.Members)
                {
                    foreach (var nextSegment in Item.Value.Segments)
                    {
                        nextSegment.Value.NodeNear.SuperPosition = new NodalLoad(
                            nextSegment.Value.NodeNear.SuperPosition.X + nextSegment.Value.NearSuperGlobal.X,
                            nextSegment.Value.NodeNear.SuperPosition.Y + nextSegment.Value.NearSuperGlobal.Y,
                            nextSegment.Value.NodeNear.SuperPosition.M + nextSegment.Value.NearSuperGlobal.M
                            );

                        nextSegment.Value.NodeFar.SuperPosition = new NodalLoad(
                            nextSegment.Value.NodeFar.SuperPosition.X + nextSegment.Value.FarSuperGlobal.X,
                            nextSegment.Value.NodeFar.SuperPosition.Y + nextSegment.Value.FarSuperGlobal.Y,
                            nextSegment.Value.NodeFar.SuperPosition.M + nextSegment.Value.FarSuperGlobal.M
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        #region Transfer from Nodes.

        internal static void GetQkFromSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Get [Qk] vector from model data.", 0);

            try
            {
                DQk = DoubleMatrix.VectorCreate(UnrestrainedDOF);
                //Get the Qk matrix.
                Qk = Matrix<double>.Build.Dense(UnrestrainedDOF, 1);
                for (int j = 0; j < UnrestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {

                        if (Item.Value.Codes.X == j)
                        {
                            Qk[j, 0] = (double)Item.Value.Load.X + (double)Item.Value.SuperPosition.X;

                            DQk[j] = (double)Item.Value.Load.X + (double)Item.Value.SuperPosition.X;
                        }
                        if (Item.Value.Codes.Y == j)
                        {
                            Qk[j, 0] = (double)Item.Value.Load.Y + (double)Item.Value.SuperPosition.Y;

                            DQk[j] = (double)Item.Value.Load.Y + (double)Item.Value.SuperPosition.Y;
                        }
                        if (Item.Value.Codes.M == j)
                        {
                            Qk[j, 0] = (double)Item.Value.Load.M + (double)Item.Value.SuperPosition.M;

                            DQk[j] = (double)Item.Value.Load.M + (double)Item.Value.SuperPosition.M;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    [Qk] vector length " + Qk.RowCount.ToString("#,###"), 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void GetQuFromSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Get [Qu] vector from model data.", 0);

            try
            {
                DQu = DoubleMatrix.VectorCreate(RestrainedDOF);
                //Get the Qu matrix.
                Qu = Matrix<double>.Build.Dense(RestrainedDOF, 1);
                for (int j = 0; j < RestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j + UnrestrainedDOF)
                        {
                            Qu[j, 0] = (double)Item.Value.Load.X;

                            DQu[j] = (double)Item.Value.Load.X;
                        }
                        if (Item.Value.Codes.Y == j + UnrestrainedDOF)
                        {
                            Qu[j, 0] = (double)Item.Value.Load.Y;

                            DQu[j] = (double)Item.Value.Load.Y;
                        }
                        if (Item.Value.Codes.M == j + UnrestrainedDOF)
                        {
                            Qu[j, 0] = (double)Item.Value.Load.M;

                            DQu[j] = (double)Item.Value.Load.M;
                        }
                    }
                }
                AddMessage(-1, -1, "    [Qu] vector length " + Qu.RowCount.ToString("#,###"), 1);
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void GetDkFromSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Get [Dk] vector from model data.", 0);

            try
            {
                //Get the Qu matrix.
                Dk = Matrix<double>.Build.Dense(RestrainedDOF, 1);
                for (int j = 0; j < RestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j + UnrestrainedDOF)
                        {
                            Dk[j, 0] = (double)Item.Value.Displacement.X;
                        }
                        if (Item.Value.Codes.Y == j + UnrestrainedDOF)
                        {
                            Dk[j, 0] = (double)Item.Value.Displacement.Y;
                        }
                        if (Item.Value.Codes.M == j + UnrestrainedDOF)
                        {
                            Dk[j, 0] = (double)Item.Value.Displacement.M;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            
            AddMessage(-1, -1, "    Dk " + Dk.ColumnCount + " " + Dk.RowCount, 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void GetDuFromSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Get [Du] vector from model data.", 0);

            try
            {
                DDu = DoubleMatrix.VectorCreate(UnrestrainedDOF);
                //Get the Qk vector.
                Du = Matrix<double>.Build.Dense(UnrestrainedDOF, 1, 0);
                for (int j = 0; j < UnrestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j)
                        {
                            Du[j, 0] = (double)Item.Value.Displacement.X;

                            DDu[j] = (double)Item.Value.Displacement.X;
                        }
                        if (Item.Value.Codes.Y == j)
                        {
                            Du[j, 0] = (double)Item.Value.Displacement.Y;

                            DDu[j] = (double)Item.Value.Displacement.Y;
                        }
                        if (Item.Value.Codes.M == j)
                        {
                            Du[j, 0] = (double)Item.Value.Displacement.M;

                            DDu[j] = (double)Item.Value.Displacement.M;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    Du " + Du.ColumnCount + " " + Du.RowCount, 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        #endregion

        #region Assemble Matrices.

        internal static void AssembleStiffnessMatrix()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Assembling global stiffness matrix. {K}", 0);

            try
            {
                //Assemble the stiffness matrix K.
                TotalDOF = Model.Nodes.Count * 3;
                DK = DoubleMatrix.MatrixCreate(TotalDOF, TotalDOF);
                K = Matrix<double>.Build.Dense(TotalDOF, TotalDOF);
                int X = 0;
                int Y = 0;

                foreach (var Item in Model.Members)
                {
                    foreach (var nextItem in Item.Value.Segments)
                    {
                        int[] enf = { nextItem.Value.NodeNear.Codes.X, nextItem.Value.NodeNear.Codes.Y, nextItem.Value.NodeNear.Codes.M, nextItem.Value.NodeFar.Codes.X, nextItem.Value.NodeFar.Codes.Y, nextItem.Value.NodeFar.Codes.M };
                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                X = enf[i];
                                Y = enf[j];
                                K[Y, X] = K[Y, X] + (double)nextItem.Value.KMatrix[j, i];

                                DK[Y][X] = DK[Y][X] + (double)nextItem.Value.KMatrix[j, i];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    Matrix size is " + TotalDOF.ToString("#,###") + " x " + TotalDOF.ToString("#,###") + ".", 1);
            AddMessage(-1, -1, "    Total number of elements within matrix is " + (TotalDOF * TotalDOF).ToString("#,###") + ".", 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void AssembleKMatrices()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Divide K matrix into four matries {K11}, {K12}, {K21}, {K22}.", 0);

            try
            {
                //Resize and fill the K** Matrices.
                K11 = Matrix<double>.Build.Dense(UnrestrainedDOF, UnrestrainedDOF);
                for (int j = 0; j < K11.RowCount; j++)
                {
                    for (int i = 0; i < K11.ColumnCount; i++)
                    {
                        K11[j, i] = K[j, i];
                    }
                }
                AddMessage(-1, -1, "    {K11} " + K11.ColumnCount.ToString("#,###") + " x " + K11.RowCount.ToString("#,###") + " = " + (K11.ColumnCount * K11.RowCount).ToString("#,###") + " elements.", 1);


                K12 = Matrix<double>.Build.Dense(UnrestrainedDOF, RestrainedDOF);
                for (int j = 0; j < K12.RowCount; j++)
                {
                    for (int i = 0; i < K12.ColumnCount; i++)
                    {
                        K12[j, i] = K[j, i + UnrestrainedDOF];
                    }
                }
                AddMessage(-1, -1, "    {K12} " + K12.ColumnCount.ToString("#,###") + " x " + K12.RowCount.ToString("#,###") + " = " + (K12.ColumnCount * K12.RowCount).ToString("#,###") + " elements.", 1);

                K21 = Matrix<double>.Build.Dense(RestrainedDOF, UnrestrainedDOF);
                for (int j = 0; j < K21.RowCount; j++)
                {
                    for (int i = 0; i < K21.ColumnCount; i++)
                    {
                        K21[j, i] = K[j + UnrestrainedDOF, i];
                    }
                }
                AddMessage(-1, -1, "    {K21} " + K21.ColumnCount.ToString("#,###") + " x " + K21.RowCount.ToString("#,###") + " = " + (K21.ColumnCount * K21.RowCount).ToString("#,###") + " elements.", 1);



                K22 = Matrix<double>.Build.Dense(RestrainedDOF, RestrainedDOF);
                for (int j = 0; j < K22.RowCount; j++)
                {
                    for (int i = 0; i < K22.ColumnCount; i++)
                    {
                        K22[j, i] = K[j + UnrestrainedDOF, i + UnrestrainedDOF];
                    }
                }
                AddMessage(-1, -1, "    {K22} " + K22.ColumnCount.ToString("#,###") + " x " + K22.RowCount.ToString("#,###") + " = " + (K22.ColumnCount * K22.RowCount).ToString("#,###") + " elements.", 1);



                DK11 = DoubleMatrix.MatrixCreate(UnrestrainedDOF, UnrestrainedDOF);
                for (int j = 0; j < DK11.Length; j++)
                {
                    for (int i = 0; i < DK11[1].Length; i++)
                    {
                        DK11[j][i] = (double)K[j, i];
                    }
                }

                DK21 = DoubleMatrix.MatrixCreate(RestrainedDOF, UnrestrainedDOF);
                for (int j = 0; j < DK21.Length; j++)
                {
                    for (int i = 0; i < DK21[1].Length; i++)
                    {
                        DK21[j][i] = (double)K[j, i];
                    }
                }

                //K is no longer nessacary so remove from memory to improve performance.
                K = null;

                // Dimension Du to prevent errors.
                Du = Matrix<double>.Build.Dense(UnrestrainedDOF, 1, 0);


            }

            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        #endregion

        #region Solve

        internal static void SolveForDu()
        {
            //Solve for the displacements.
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Solve For Du", 0);
            //AddMessage(-1, -1, "    Du " + Du.ColumnCount + " " + Du.RowCount,1);
            //AddMessage(-1, -1, "    Qk " + Qk.ColumnCount + " " + Qk.RowCount,1);
            //AddMessage(-1, -1, "    K11 " + K11.ColumnCount + " " + K11.RowCount,1);

            try
            {
                Du = K11.Inverse() * Qk;
                DDu = DoubleMatrix.SystemSolve(DK11, DQk);
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }

            AddMessage(-1, -1, "    Du " + Du.ColumnCount + " " + Du.RowCount, 1);
            AddMessage(-1, -1, "    Qk " + Qk.ColumnCount + " " + Qk.RowCount, 1);
            AddMessage(-1, -1, "    K11 " + K11.ColumnCount + " " + K11.RowCount, 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void SolveForQu()
        {
            //Solve for the reactions.
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Solve For Qu", 0);

            try
            {
                Qu = K21 * Du;
                DQu = DoubleMatrix.MatrixVectorProduct(DK21, DDu);
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        #endregion

        #region Transfer to Nodes.

        internal static void SetQkToSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Set [Qk] vector from model data.", 0);

            try
            {
                //Set the Qk matrix.
                for (int j = 0; j < UnrestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j)
                        {
                            Item.Value.JointLoad = new NodalLoad((decimal)Qk[j, 0], Item.Value.JointLoad.Y, Item.Value.JointLoad.M);
                        }
                        if (Item.Value.Codes.Y == j)
                        {
                            Item.Value.JointLoad = new NodalLoad(Item.Value.JointLoad.X, (decimal)Qk[j, 0], Item.Value.JointLoad.M);
                        }
                        if (Item.Value.Codes.M == j)
                        {
                            Item.Value.JointLoad = new NodalLoad(Item.Value.JointLoad.X, Item.Value.JointLoad.Y, (decimal)Qk[j, 0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    [Qk] vector length " + Qk.RowCount.ToString("#,###"), 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void SetQuToSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Set [Qu] vector from model data.", 0);

            try
            {
                //Set the Qu matrix.
                for (int j = 0; j < RestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j + UnrestrainedDOF)
                        {
                            Item.Value.LoadReaction = new NodalLoad((decimal)Qu[j, 0], Item.Value.LoadReaction.Y, Item.Value.LoadReaction.M);
                        }
                        if (Item.Value.Codes.Y == j + UnrestrainedDOF)
                        {
                            Item.Value.LoadReaction = new NodalLoad(Item.Value.LoadReaction.X, (decimal)Qu[j, 0], Item.Value.LoadReaction.M);
                        }
                        if (Item.Value.Codes.M == j + UnrestrainedDOF)
                        {
                            Item.Value.LoadReaction = new NodalLoad(Item.Value.LoadReaction.X, Item.Value.LoadReaction.Y, (decimal)Qu[j, 0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    [Qu] vector length " + Qu.RowCount.ToString("#,###"), 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void SetDkToSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Set [Dk] vector from model data.", 0);

            try
            {
                //Set the Dk matrix. These should be zeros as the fixed dof's are fixed.
                for (int j = 0; j < RestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j + UnrestrainedDOF)
                        {
                            Item.Value.Displacement = new Point((decimal)Dk[j, 0], Item.Value.Displacement.Y, Item.Value.Displacement.M);
                        }
                        if (Item.Value.Codes.Y == j + UnrestrainedDOF)
                        {
                            Item.Value.Displacement = new Point(Item.Value.Displacement.X, (decimal)Dk[j, 0], Item.Value.Displacement.M);
                        }
                        if (Item.Value.Codes.M == j + UnrestrainedDOF)
                        {
                            Item.Value.Displacement = new Point(Item.Value.Displacement.X, Item.Value.Displacement.Y, (decimal)Dk[j, 0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    [Dk] vector length " + Dk.RowCount.ToString("#,###"), 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        /// <summary>
        /// After Solver to Du the results are to be transfered back to the vector.
        /// </summary>
        internal static void SetDuToSubNodes()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Set [Du] vector from model data.", 0);

            try
            {
                for (int j = 0; j < UnrestrainedDOF; j++)
                {
                    foreach (var Item in Model.Nodes)
                    {
                        if (Item.Value.Codes.X == j)
                        {
                            Item.Value.Displacement = new Point((decimal)Du[j, 0], Item.Value.Displacement.Y, Item.Value.Displacement.M);
                        }
                        if (Item.Value.Codes.Y == j)
                        {
                            Item.Value.Displacement = new Point(Item.Value.Displacement.X, (decimal)Du[j, 0], Item.Value.Displacement.M);
                        }
                        if (Item.Value.Codes.M == j)
                        {
                            Item.Value.Displacement = new Point(Item.Value.Displacement.X, Item.Value.Displacement.Y, (decimal)Du[j, 0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }
            AddMessage(-1, -1, "    [Du] vector length " + Du.RowCount.ToString("#,###"), 1);
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        #endregion

        internal static void UpdateMemberAndSegmentsFromMatrix()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Update Members and Segments from Matrix.", 0);

            try
            {
                foreach (var Item in Model.Members)
                {
                    foreach (var nextSegment in Item.Value.Segments)
                    {
                        nextSegment.Value.UpdatePropertiesFromMatrix();
                    }
                    Item.Value.UpdatePropertiesFromMatrix();
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 2);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 2);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void CreateLengthRatioColors()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Calculate Length Ratios.", 0);

            try
            {
                if (Camera.LargestLengthRatio == 0)
                {
                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            nextSegment.Value.LengthRatioColor = Color.FromArgb(255, 255, 255, 255);
                            nextSegment.Value.UpdateColor();
                        }
                    }
                }
                else
                {
                    decimal compression_ratio = 255 / Camera.LargestLengthRatio;
                    decimal tension_ratio = 255 / Camera.LargestLengthRatio;
                    byte RatioByte;

                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            if (nextSegment.Value.LengthRatio > 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.LengthRatio) * tension_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }
                                RatioByte = (byte)(255 - tmpValue);
                                nextSegment.Value.LengthRatioColor = Color.FromArgb(255, RatioByte, RatioByte, 255);
                            }
                            else if (nextSegment.Value.LengthRatio < 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.LengthRatio) * compression_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }
                                RatioByte = (byte)(255 - tmpValue);
                                nextSegment.Value.LengthRatioColor = Color.FromArgb(255, 255, RatioByte, RatioByte);
                            }
                            else
                            {
                                nextSegment.Value.LengthRatioColor = Color.FromArgb(255, 255, 255, 255);
                            }
                            nextSegment.Value.UpdateColor();
                        }
                    }
                }

                Camera.UpdateSegmentColor(1);
            }
            catch (Exception ex)
            {
                HasWarning = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 3);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 3);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }


        internal static void CreateAxialRatioColors()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Calculate Axial Ratios.", 0);

            try
            {
                if (Camera.LargestAxialRatio == 0)
                {
                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            nextSegment.Value.AxialRatioColor = Color.FromArgb(255, 255, 255, 255);
                            nextSegment.Value.UpdateColor();
                        }
                    }
                }
                else
                {
                    decimal compression_ratio = 255 / Camera.LargestAxialRatio;
                    decimal tension_ratio = 255 / Camera.LargestAxialRatio;
                    byte RatioByte;

                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            if (nextSegment.Value.InternalLoadNearLocal.X < 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.InternalLoadNearLocal.X) * tension_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }
                                RatioByte = (byte)(255 - tmpValue);
                                nextSegment.Value.AxialRatioColor = Color.FromArgb(255, RatioByte, RatioByte, 255);
                            }
                            else if (nextSegment.Value.InternalLoadNearLocal.X > 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.InternalLoadNearLocal.X) * compression_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }
                                RatioByte = (byte)(255 - tmpValue);
                                nextSegment.Value.AxialRatioColor = Color.FromArgb(255, 255, RatioByte, RatioByte);
                            }
                            else
                            {
                                nextSegment.Value.AxialRatioColor = Color.FromArgb(255, 255, 255, 255);
                            }
                            nextSegment.Value.UpdateColor();
                        }
                    }
                }
         
                Camera.UpdateSegmentColor(1);
            }
            catch (Exception ex)
            {
                HasWarning = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 3);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 3);
            }
            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void CreateNormalStressColors()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Calculate Axial Ratios.", 0);

            try
            {
                if (Camera.LargestNormalStress == 0)
                {
                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            nextSegment.Value.NormalStressColor = Color.FromArgb(255, 255, 255, 255);
                            nextSegment.Value.UpdateColor();
                        }
                    }
                }
                else
                {
                    decimal compression_ratio = 255 / Camera.LargestNormalStress;
                    decimal tension_ratio = 255 / Camera.LargestNormalStress;

                    byte RatioByte;

                    foreach (var Item in Model.Members)
                    {
                        foreach (var nextSegment in Item.Value.Segments)
                        {
                            if (nextSegment.Value.NormalStress < 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.NormalStress) * tension_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }

                                //Debug.WriteLine("tmpValue " + tmpValue);
                                RatioByte = (byte)(255 - tmpValue);

                                nextSegment.Value.NormalStressColor = Color.FromArgb(255, RatioByte, RatioByte, 255);
                            }
                            else if (nextSegment.Value.NormalStress > 0)
                            {
                                int tmpValue = (int)(Math.Abs(nextSegment.Value.NormalStress) * compression_ratio);
                                if (tmpValue > 255) { tmpValue = 255; }
                                if (tmpValue < 0) { tmpValue = 0; }

                                //Debug.WriteLine("tmpValue " + tmpValue);
                                RatioByte = (byte)(255 - tmpValue);

                                nextSegment.Value.NormalStressColor = Color.FromArgb(255, 255, RatioByte, RatioByte);
                            }
                            else
                            {
                                nextSegment.Value.NormalStressColor = Color.FromArgb(255, 255, 255, 255);
                            }

                        }
                    }
                }
                
                Camera.UpdateSegmentColor(1);
            }
            catch (Exception ex)
            {
                HasWarning = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 3);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 3);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void CalculateEqulibriumValues()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Calculate Equlibrium Values.", 0);

            try
            {
                foreach (var Item in Model.Nodes.NodesWithNodalLoads)
                {
                    Model.ForceX += Item.Value.Load.X;
                    Model.ForceY += Item.Value.Load.Y;
                    Model.ForceM += Item.Value.Load.M;
                }

                foreach (var Item in Model.Nodes.NodesWithReactions)
                {
                    Model.ReactionX += Item.Value.LoadReaction.X;
                    Model.ReactionY += Item.Value.LoadReaction.Y;
                    Model.ReactionM += Item.Value.LoadReaction.M;
                }
            }
            catch (Exception ex)
            {
                HasWarning = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 3);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 3);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static void CalculateMaterials()
        {

        }

        internal static void CalculateLabour()
        {

        }

        internal static void CalculateCost()
        {
            Stopwatch TaskTimer = new Stopwatch();
            TaskTimer.Start();
            AddMessage(MainTimer.ElapsedMilliseconds, -1, "Calculate Equlibrium Values.", 0);

            try
            {
                foreach (var Item in Model.Members)
                {
                    Model.TotalCost += Item.Value.MemberCost;
                    Model.MaterialCost += Item.Value.MaterialCost;
                    Model.NodeCost += Item.Value.NodeCost;
                }
            }

            catch (Exception ex)
            {
                HasWarning = true;
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    ERROR!", 3);
                AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    " + ex.Message, 3);
            }

            AddMessage(MainTimer.ElapsedMilliseconds, TaskTimer.ElapsedMilliseconds, "    Finished.", 1);
        }

        internal static async void DisplayEndMessages()
        {
            AddMessage(-1, -1, "    ", 0);
            AddMessage(MainTimer.ElapsedMilliseconds, MainTimer.ElapsedMilliseconds, "    Finished.", 0);
            
            if (!HasErrors)
            {
                if (Options.AutoFinishSolver)
                {

                    if (!HasWarning)
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           Frame rootFrame = Window.Current.Content as Frame;
                           rootFrame.Navigate(typeof(Results));
                       }
                       );
                    }
                    else
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           PanelSolver.Current.ShowResultsButton();
                       }
                       );
                    }
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           PanelSolver.Current.ShowResultsButton();
                       }
                       );
                }
            }
        }

    }
}
