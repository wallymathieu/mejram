# Mejram [![Build status](https://ci.appveyor.com/api/projects/status/jypcjbt3ipbwmq4x/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/mejram/branch/master)
## The name

Mejram is the swedish name for Marjoram. Like any spice it is intended to give flavor to the analysis of sql databases.

## Goal

The goal is to be able to analyse relational databases with a large number of tables. Many of the databases I've seen related to managing a specific domain (enterprise system) have around 200-300 tables. Even though this is not to large to analyse by hand, it can definitelly be to cumbersome. If you use sql server and the relations are well maintained. That is: The referential constraints are present where they should be. Then you don't need to use this tool. You can use sql server manager. The problem is that it's often a mixture of well maintained referential constraints and implied (i.e. can be gleaned by the naming convention in the database) but not formal.

## Licence
GNU Lesser Public License.
