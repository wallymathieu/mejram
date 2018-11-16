module MejramFs.Models

type ColumnKey={TableName:string;ColumnName:string}
type Column={ColumnKey:ColumnKey;Number:int16;ColumnType:string;NotNullConstraint:bool}

type PrimaryKeyConstraint={PrimaryKeyName:string; PrimaryKeys: ColumnKey list}

type ForeignKeyColumn={From:ColumnKey;To:ColumnKey }
type ForeignKeyConstraint= {ForeignKeyName:string; ForeignKeys: ForeignKeyColumn list}

type Table={
  Columns: Column list
  PrimaryKey: PrimaryKeyConstraint 
  ForeignKeys: ForeignKeyConstraint list}

type Database ={ Tables: Table list }

type Vertex<'t>={Data:'t; Weight:double}
type Edge<'t>={FromVertex:Vertex<'t>; ToVertex:Vertex<'t>; Weight:double; IsDirected:bool}

