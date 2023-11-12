using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

///<summary> A simple lookup table that generates guaranteed unique keys. Thread Safe. </summary>
public class ActionLookupTable<E>
{

  private long lastTimeStamp=-1;
  Dictionary<long,Action<E>> table = new Dictionary<long, Action<E>>();
	
	///<returns> A guaranteed unique long for the given Action. Use this to Get or Pull the Action.</returns>
	public long Put(Action<E> action)
  {
		long id = GetUniqueTick();
		
    lock(table)
    {
			table.Add(id,action);
		}
		
    return id;
	}
	
	
	public Action<E> Get(long id)
  {
		Action<E> action;
		lock(table)
    {
			if(table.TryGetValue(id,out action))
      {
        return action;
      }
		}
    
		return null;
	}
	
	///<summary>Removes and Returns the Action for the given id.</summary>
	///<returns> Action or null if invalid key</returns>
    public Action<E> Pull(long id)
    {
      Action<E> action;
		  lock(table)
      {
			  if(table.TryGetValue(id,out action))
        {
				  table.Remove(id);
				  return action;
			  }
		}
        return null;
    }

    //Adapted from https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked.compareexchange?view=netframework-4.8 
    private long GetUniqueTick()
    {
        long initialValue, newValue;
      
        do
        {
            initialValue =lastTimeStamp;
            long now = DateTime.UtcNow.Ticks;
            newValue = Math.Max(now,initialValue +1);
            // CompareExchange compares lastTimeStamp to initialValue. If
            // they are not equal, then another thread has updated the
            // running total since this loop started. CompareExchange
            // does not update lastTimeStamp. CompareExchange returns the
            // contents of lastTimeStamp, which do not equal initialValue,
            // so the loop executes again.
        } while(Interlocked.CompareExchange(ref lastTimeStamp,newValue,initialValue ) != initialValue );
        // If no other thread updated the running total, then 
        // lastTimeStamp and initialValue are equal when CompareExchange
        // compares them, and newValue is stored in lastTimeStamp.
        // CompareExchange returns the value that was in lastTimeStamp
        // before the update, which is equal to initialValue, so the 
        // loop ends.

        // The function returns newValue, not lastTimeStamp, because
        // lastTimeStamp could be changed by another thread between
        // the time the loop ends and the function returns.
        return newValue;
    }
}
