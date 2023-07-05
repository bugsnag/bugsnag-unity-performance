namespace BugsnagUnityPerformance
{
    /**
     * Phased startup ensures that components can rely on the fact that other
     * components are at a similar phase of startup when running certain kinds of code.
     * 
     * Each IPhasedStartup object is responsible for passing IPhasedStartup
     * messages to sub-objects that it owns (ownership meaning that this is the
     * object that actually instantiated the sub-object). If the IPhasedStartup
     * object was passed in via a constructor, YOU DO NOT OWN IT.
     * 
     * It consists of the following phases:
     * 
     * 1. Construction
     * 
     * - Call only constructors to construct sub-objects and their dependents
     *   (building the object graph but not calling it).
     * Note: Do not call methods on other IPhasedStartup objects.
     * 
     * 2. Configuration
     * 
     * - Read the configuration and store relevant information that this object will require.
     * - Get this object into a state where any of its public methods can be called without breaking.
     * - Pass the Configure message to any IPhasedStartup objects owned by this object.
     * Note: Do not call methods on other IPhasedStartup objects (other than Configure)
     *       because we can't assume that any other objects have been configured yet.
     * 
     * 3. Start
     * 
     * - Start this component and any owned IPhasedStartup subobjects.
     *   Most objects will do nothing here, but if you need to start threads,
     *   register OS callbacks etc, this is the place to do it.
     * - Pass the Start message to any IPhasedStartup objects owned by this object.
     * Note: It's safe to call other IPhasedStartup objects.
     */
    interface IPhasedStartup
    {
        void Configure(PerformanceConfiguration config);
        void Start();
    }
}
