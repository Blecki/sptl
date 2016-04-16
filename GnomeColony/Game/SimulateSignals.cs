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

        public List<DeviceSignalActivation> SimulateSignals(List<DeviceSignalActivation> PreviousActivations)
        {
            var openWires = new List<Tuple<bool, Wire>>();
            var activations = new List<DeviceSignalActivation>();
            var previousSimulation = SimulationID;
            SimulationID += 1;
            var currentSimulation = SimulationID;

            WireMap.ForEachCellInWorldRect(0, 0, WireMap.PixelWidth, WireMap.PixelHeight,
                (w, x, y) =>
                {
                    if (w.Cell != null && w.Cell.Source)
                    {
                        //w.Signal = currentSimulation;
                        openWires.Add(Tuple.Create(false, w));
                    }
                });

            foreach (var activation in PreviousActivations)
                if (activation.Condition == null || activation.Condition(this))
                    openWires.Add(Tuple.Create(false, WireMap.GetCellUnsafe(activation.Location)));

           
                while (openWires.Count > 0)
                {
                    var open = openWires[0];
                    var nextWire = open.Item2;
                    openWires.RemoveAt(0);
                    
                if (nextWire.Signal == currentSimulation) continue;

                    nextWire.Signal = currentSimulation;

                    if (open.Item1 && nextWire.Cell != null)
                    {
                        if (nextWire.Device.OnSignal != null)
                        {
                            var newActivations = nextWire.Device.OnSignal(currentSimulation, nextWire, this);
                            foreach (var c in newActivations)
                                activations.Add(c);
                        }
                    }

                    if ((nextWire.Connections & Wire.UP) == Wire.UP)
                        openWires.Add(Tuple.Create(true, WireMap.GetCell(nextWire.Coordinate.X, nextWire.Coordinate.Y - 1)));
                    if ((nextWire.Connections & Wire.DOWN) == Wire.DOWN)
                        openWires.Add(Tuple.Create(true, WireMap.GetCell(nextWire.Coordinate.X, nextWire.Coordinate.Y + 1)));
                    if ((nextWire.Connections & Wire.RIGHT) == Wire.RIGHT)
                        openWires.Add(Tuple.Create(true, WireMap.GetCell(nextWire.Coordinate.X + 1, nextWire.Coordinate.Y)));
                    if ((nextWire.Connections & Wire.LEFT) == Wire.LEFT)
                        openWires.Add(Tuple.Create(true, WireMap.GetCell(nextWire.Coordinate.X - 1, nextWire.Coordinate.Y)));
                }

            
            WireMap.ForEachChunk((c, x, y) => c.InvalidateMesh());

            return activations;
        }
    }
}
