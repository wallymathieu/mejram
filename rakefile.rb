require 'albacore'
task :test => ["py:test", "cs:test", "rb:test"]

import 'rakefile_cs.rb'

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
  #TODO: Use rake test runner
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
