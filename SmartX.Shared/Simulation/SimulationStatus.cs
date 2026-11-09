namespace SmartX.Shared.Simulation;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record SimulationStatus(bool Running, SimulationScenario Scenario, int Devices, long Ticks, int Seed);
