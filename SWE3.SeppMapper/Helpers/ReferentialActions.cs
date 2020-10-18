namespace SWE3.SeppMapper.Helpers
{
    public enum ReferentialActions
    {
        /// <summary>
        /// Whenever rows in the parent (referenced) table are deleted (or updated), the respective rows of the child (referencing)
        /// table with a matching foreign key column will be deleted (or updated) as well.
        /// </summary>
        Cascade,
        /// <summary>
        /// A value cannot be updated or deleted when a row exists in a referencing or child table that references the value in the referenced table. 
        /// </summary>
        Restrict,
        /// <summary>
        /// When an UPDATE or DELETE statement is executed on the referenced table using the referential action NO ACTION,
        /// the DBMS verifies at the end of the statement execution that none of the referential relationships are violated.
        /// </summary>
        NoAction,
        /// <summary>
        /// In general, the action taken by the DBMS for SET NULL or SET DEFAULT is the same for both ON DELETE or ON UPDATE: The value of the affected referencing attributes is changed to NULL.
        /// </summary>
        SetNull
    }
}