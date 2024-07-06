// See https://aka.ms/new-console-template for more information

using DepResolve;
public record Dependency(string name, string version);

(List<Dependency> direct, List<(Dependency source, IEnumerable<Dependency> dependency)> subDependencies) GetDependecies(string[] lines)
{

    if (lines == null) throw new ArgumentNullException(nameof(lines));

    var subDependenciesCount = 0;

    // is empty file
    if (lines.Length == 0) throw new NotSupportedException("Empty example is not allowed.");

    // is there number in the first line
    if (!int.TryParse(lines[0], out var directDependenciesCount))
        throw new ArgumentOutOfRangeException(nameof(lines), "Cannot parse dependencies count.");

    // there second number and if is the second number a number
    if (lines.Length > directDependenciesCount + 1 &&
        !int.TryParse(lines[directDependenciesCount + 1], out subDependenciesCount))
        throw new ArgumentOutOfRangeException(nameof(lines), "Cannot parse sub-dependencies count.");

    var directDepts = lines.Skip(1).Take(directDependenciesCount).Select(x => x.Split(',')).ToList();
    if (directDepts.Any(x => x.Length != 2))
        throw new ArgumentException(nameof(lines), "Cannot parse root dependency.");

    // select throw is done here only to show that I can optimize
    var direct = directDepts.GroupBy(x => x[0], x => x[1])
        .Select(x=> 
            x.Count() == 1 ? new Dependency(x.Key, x.First())
                : throw new ArgumentException("Some direct dependencies are in version conflict.")).ToList();

    var subDepts = lines.Skip(2 + directDependenciesCount).Take(subDependenciesCount)
        .Select(x => x.Split(',').Chunk(2).ToList())
        .Select(x =>
            (source: new Dependency(x[0][0], x[0][1]),
                dep: x.Skip(1).Select(z => new Dependency(z[0], z[1]))))
        .ToList();

    return (direct, subDepts);
}

bool CheckIfDependenciesMatch(string[] lines)
{
    // this can be an made an object with interface and implementation if you want to do IoC,
    // or it can be handler (class or function) if you want to do mediatr. 
    var (directDependencies, subDependencies) = GetDependecies(lines);

    var dependencyDictionary = directDependencies.ToDictionary(x => x.name, x => x.version);

    // queue because linked list or ring buffer so n(1) operations
    var subDeptsQueue = new Queue<(Dependency source, IEnumerable<Dependency> deps)>(subDependencies);

    // when this hits the length of the whole remaining list, it is time to let go and end.
    var attemptCounter = 0;
    while (subDeptsQueue.Count > attemptCounter)
    {
        var current = subDeptsQueue.Dequeue(); // take fifo

        // we ignore recurrent dependency definitions(chains) when the source dependency is not used
        // or the definition describes a different version
        // this is noise cancellation of sort.
        if (!dependencyDictionary.ContainsKey(current.source.name) 
            || dependencyDictionary[current.source.name] != current.source.version)
        {
            // make sure we marked we've tried a thing on a list and decided to ignore it.
            attemptCounter++;

            // we're putting it back on the queue but on the end.
            // might be it will be used later as dependency of some other dependency.
            subDeptsQueue.Enqueue(current);
            continue;
        }

        foreach (var dep in current.deps)
        {
            // has this package already covered
            if (!dependencyDictionary.TryGetValue(dep.name, out var versionFound))
            {
                // no? add
                dependencyDictionary[dep.name] = dep.version;
            }
            else
            {
                // yes? make sure it's not different version or else shout this case fails.
                if (versionFound != dep.version) return false;
            }
        }

        //if we've added something to the final dict then we reset our attempt counter.
        attemptCounter = 0;
    }

    return true;
}

foreach (var i in Enumerable.Range(0, 10))
    try
    {
        var inputLines = File.ReadAllLines($"./testdata/input{i:000}.txt").RemoveEmptyLines();
        var outputTest = File.ReadAllLines($"./testdata/output{i:000}.txt").RemoveEmptyLines();
    
        var dependencyParseResult = CheckIfDependenciesMatch(inputLines);
        var expectedResult = (outputTest.Length > 0 ? outputTest[0] : null) switch
        {
            "PASS" => true,
            "FAIL" => false,
            _ => throw new ArgumentOutOfRangeException(nameof(outputTest), $"Cannot parse output for example {i}.")
        };

        // here you can extend to list the versions required.
        var actualResult = dependencyParseResult; // dependencyParseResult.All(x=>x.versions.Count == 1);


        Console.WriteLine(
            $"Input: {i} \t Expected pass: {expectedResult} \t actual pass: {actualResult} \t match?: {expectedResult == actualResult}");
    }
    catch
    {
        Console.WriteLine($"Example {i} cannot be read.");
    }


