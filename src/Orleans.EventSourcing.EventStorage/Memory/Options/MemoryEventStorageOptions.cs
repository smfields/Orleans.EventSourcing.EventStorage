using Orleans.Storage;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class MemoryEventStorageOptions : IStorageProviderSerializerOptions
{
    /// <summary>
    /// Default number of queue storage grains.
    /// </summary>
    public const int NumStorageGrainsDefaultValue = 10;

    /// <summary>
    /// Default init stage
    /// </summary>
    public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;

    /// <summary>
    /// Gets or sets the number of store grains to use.
    /// </summary>
    public int NumStorageGrains { get; set; } = NumStorageGrainsDefaultValue;

    /// <summary>
    /// Gets or sets the stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = DEFAULT_INIT_STAGE;

    /// <inheritdoc/>
    public IGrainStorageSerializer? GrainStorageSerializer { get; set; }
}