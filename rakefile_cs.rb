require 'albacore'
require 'nuget_helper'
namespace :cs do

  dir = File.dirname(__FILE__)

  desc "build using msbuild"
  build :build do |msb|
    msb.prop :configuration, :Debug
    msb.target = [:Rebuild]
    msb.logging = 'minimal'
    msb.sln =File.join(dir, "Mejram.sln")
  end

  build :build_release do |msb|
    msb.prop :configuration, :Release
    msb.target = [:Rebuild]
    msb.logging = 'minimal'
    msb.sln =File.join(dir, "Mejram.sln")
  end

  desc "Install missing NuGet packages."
  task :restore do
    NugetHelper.exec("restore ./Mejram.sln")
  end

  desc "test using console"
  test_runner :test => [:build] do |runner|
    runner.exe = NugetHelper.nunit_path
    files = Dir.glob(File.join(File.dirname(__FILE__),
      "*Tests","bin","Debug","*Tests.dll"))
    runner.files = files 
  end

end
