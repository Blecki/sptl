using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public partial class Play
    {
        /// <summary>
        /// Simulate 'current' using the SIGNAL abstraction, as in games like MINECRAFT.
        /// </summary>

        private static int SimulationID = 2;

        private void TryOpenWire(Wire w, int CurrentSimulation, List<Wire> OpenWires, List<DeviceSignalActivation> Activations)
        {
            if (w.Signal != CurrentSimulation)
            {
                w.Signal = CurrentSimulation;
                if (w.Cell != null)
                {
                    if (w.Device.OnSignal != null)
                    {
                        var newActivations = w.Device.OnSignal(CurrentSimulation, w, this);
                        foreach (var c in newActivations)
                        {
                            if (c.Condition(this))
                                OpenWires.Add(WireMap.GetCellUnsafe(c.Location));
                            else
                                Activations.Add(c);
                        }
                    }
                }
                else
                    OpenWires.Add(w); // No device - just open it.
            }
        }


        private void TryOpenWireIgnoreDevice(Wire w, int CurrentSimulation, List<Wire> OpenWires)
        {
            if (w.Signal != CurrentSimulation)
            {
                w.Signal = CurrentSimulation;
                OpenWires.Add(w); // No device - just open it.
            }
        }

        public void SimulateSignals()
        {
            var openWires = new List<Wire>();
            var activations = new List<DeviceSignalActivation>();
            var currentSimulation = SimulationID;
            SimulationID += 1;

            WireMap.ForEachCellInWorldRect(0, 0, WireMap.PixelWidth, WireMap.PixelHeight,
                (w, x, y) =>
                {
                    if (w.Cell != null && w.Cell.Source)
                    {
                        //w.Signal = currentSimulation;
                        openWires.Add(w);
                    }
                });

            while (true)
            {

                while (openWires.Count > 0)
                {
                    var nextWire = openWires[0];
                    openWires.RemoveAt(0);

                    if (nextWire.Signal == currentSimulation) continue;

                    nextWire.Signal = currentSimulation;
                    if (nextWire.Cell != null)
                    {
                        if (nextWire.Device.OnSignal != null)
                        {
                            var newActivations = nextWire.Device.OnSignal(currentSimulation, nextWire, this);
                            foreach (var c in newActivations)
                                activations.Add(c);
                        }
                    }

                    if ((nextWire.Connections & Wire.UP) == Wire.UP)
                        openWires.Add(WireMap.GetCell(nextWire.Coordinate.X, nextWire.Coordinate.Y - 1));
                    if ((nextWire.Connections & Wire.DOWN) == Wire.DOWN)
                        openWires.Add(WireMap.GetCell(nextWire.Coordinate.X, nextWire.Coordinate.Y + 1));
                    if ((nextWire.Connections & Wire.RIGHT) == Wire.RIGHT)
                        openWires.Add(WireMap.GetCell(nextWire.Coordinate.X + 1, nextWire.Coordinate.Y));
                    if ((nextWire.Connections & Wire.LEFT) == Wire.LEFT)
                        openWires.Add(WireMap.GetCell(nextWire.Coordinate.X - 1, nextWire.Coordinate.Y));
                }

                var activationsTriggered = 0;

                for (var a = 0; a < activations.Count;)
                {
                    var activation = activations[a];
                    if (activation.Condition(this))
                    {
                        var wire = WireMap.GetCellUnsafe(activation.Location);
                        openWires.Add(wire);
                        
                        activations.RemoveAt(a);

                        activationsTriggered += 1;
                    }
                    else
                        a += 1;
                }

                if (activationsTriggered == 0) break;
            }
            
            WireMap.ForEachChunk((c, x, y) => c.InvalidateMesh());
        }
    }
}
