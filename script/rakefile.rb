task :test => ["rb_test"] # "py_test"
$dir = File.dirname(__FILE__)
desc "python test"
task :py_test do
    Dir.glob(File.join($dir, '**','*.py')).each do |file| 
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
desc "ruby test"
task :rb_test do
    Dir.glob(File.join($dir, '**','*.rb')).select do |file|
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
