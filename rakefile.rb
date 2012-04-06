require 'albacore'
task :test => ["ms:test","py:test","rb:test"]

namespace :ms do
    msbuild :build do |msb|
        msb.properties :configuration => :Debug
        msb.targets :Clean, :Rebuild
        msb.verbosity = 'quiet'
        msb.solution = "Mejram.simple.sln"
    end
    desc "test using nunit console"
    nunit :test => :build do |nunit|
        nunit.command = "C:\\Program Files\\NUnit 2.6\\bin\\nunit-console.exe"
        nunit.assemblies "Mejram.Tests/bin/Debug/Mejram.Tests.dll"
    end
end

namespace :py do
    task :test do
        Dir.glob(File.join('**','*.py')).each do |file| 
            puts file
            cd File.dirname(file) do
                sh "python #{File.basename(file)}" do |ok, res|
                    if !ok
                        raise "!Failed"
                    end
                end
            end
        end
    end 

end

namespace :rb do
    task :test do
        Dir.glob(File.join('**','*.rb')).select do |file|
            file.index('_tests.rb') || file.index('_test.rb')
        end.each do |file| 
            puts file
            cd File.dirname(file) do
                sh "ruby #{File.basename(file)}" do |ok, res|
                    if !ok
                        raise "!Failed"
                    end
                end
            end
        end
    end 
end
