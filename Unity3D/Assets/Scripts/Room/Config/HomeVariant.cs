using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Home.Core
{
    //Compoent 变体方案：增量
    public class VariantUtils
    {
        public static void Variants(object variant, object source, object change)
        {
            FieldInfo[] fields = variant.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in fields)
            {
                string name = p.Name;
                PropertyInfo sorceProperty = source.GetType().GetProperty(name);
                PropertyInfo changeProperty = change.GetType().GetProperty(name);
                object sourceValue = sorceProperty.GetValue(source);
                object changeValue = changeProperty.GetValue(change);
                string sourceStr = JsonUtility.ToJson(sourceValue);
                string changeStr = JsonUtility.ToJson(changeValue);
                if (sourceValue.ToString() != changeValue.ToString() || sourceStr != changeStr)
                {
                    object value = p.GetValue(variant);
                    value.GetType().GetField("use").SetValue(value, true);
                    value.GetType().GetField("value").SetValue(value, changeValue);
                }
            }
        }
        
        public static bool HasChange(object variant)
        {
            FieldInfo[] property = variant.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in property)
            {
                object value = p.GetValue(variant);
                if ((bool) value.GetType().GetField("use").GetValue(value))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static void Serialize(JObject jObject, object variant)
        {
            FieldInfo[] property = variant.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in property)
            {
                object value = p.GetValue(variant);
                if ((bool) value.GetType().GetField("use").GetValue(value))
                {
                    jObject[p.Name] =  JObject.Parse(JsonUtility.ToJson(value));
                }
            }
        }

        public static void Deserialize(object variant, JObject json)
        {
            foreach (var p in json)
            {
                FieldInfo fileInfo = variant.GetType().GetField(p.Key);
                object value = fileInfo.GetValue(variant);
                value.GetType().GetField("use").SetValue(value, true);
                FieldInfo valueFiledInfo = value.GetType().GetField("value");
                try
                {
                    object coverValue;
                    if (valueFiledInfo.FieldType == typeof(bool)) { coverValue = Convert.ToBoolean(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(byte)) { coverValue = Convert.ToByte(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(char)) { coverValue = Convert.ToChar(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(decimal)) { coverValue = Convert.ToDecimal(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(double)) { coverValue = Convert.ToDouble(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(float)) { coverValue = Convert.ToSingle(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(int)) { coverValue = Convert.ToInt32(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(long)) { coverValue = Convert.ToInt64(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(sbyte)) { coverValue = Convert.ToSByte(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(short)) { coverValue = Convert.ToInt16(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(uint)) { coverValue = Convert.ToUInt32(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(ulong)) { coverValue = Convert.ToUInt64(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(ushort)) { coverValue = Convert.ToUInt16(p.Value["value"]); }
                    else if (valueFiledInfo.FieldType == typeof(string)) { coverValue = Convert.ToSingle(p.Value["value"]); }
                    else if (!p.Value["value"].ToString().Contains("{"))//enum
                    {
                        JObject tempObj = new JObject();
                        tempObj["value__"] = p.Value["value"];
                        coverValue = JsonUtility.FromJson(tempObj.ToString(), valueFiledInfo.FieldType);
                    }
                    else { coverValue = JsonUtility.FromJson(p.Value["value"].ToString(), valueFiledInfo.FieldType); }
                    valueFiledInfo.SetValue(value, coverValue);
                }
                catch (Exception e)
                {
                    Debug.LogError($"can not Deserialize {valueFiledInfo.FieldType} {p.Value["value"]}");
                }
            }
        }

        public static void Cover(object variant, object dest)
        {
            try
            {
                FieldInfo[] property = variant.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var p in property)
                {
                    object value = p.GetValue(variant);
                    if ((bool) value.GetType().GetField("use").GetValue(value))
                    {
                        object coverValue = value.GetType().GetField("value").GetValue(value);
                        PropertyInfo propertyInfo = dest.GetType().GetProperty(p.Name, BindingFlags.Instance | BindingFlags.Public);
                        propertyInfo.SetValue(dest, coverValue);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
    
    [Serializable]
    public class VariantMember<T>
    {
        public bool use;
        public T value;
    }

    [Serializable]
    public class PSBurst
    {
        public float time;
        public ParticleSystem.MinMaxCurve count;
        public int cycleCount;
        public float repeatInterval;
        public float probability;

        public void Parse(ParticleSystem.Burst burst)
        {
            time = burst.time;
            count = burst.count;
            cycleCount = burst.cycleCount;
            repeatInterval = burst.repeatInterval;
            probability = burst.probability;
        }
        
        public ParticleSystem.Burst Coverto()
        {
            ParticleSystem.Burst burst = new ParticleSystem.Burst();
            burst.time = time;
            burst.count = count;
            burst.cycleCount = cycleCount;
            burst.repeatInterval = repeatInterval;
            burst.probability = probability;
            return burst;
        }
    }
    
    [Serializable]
    public class PSSpecialVariant
    {
        public bool use_bursts;
        public List<PSBurst> bursts = new List<PSBurst>();

        public bool use_shareMaterial;
        public string shareMaterial;

        public bool use_trailMaterial;
        public string trailMaterial;

        public void Variants(ParticleSystem source, ParticleSystem change)
        {
            if (null == source || null == change) return;
            
            //bursts
            {
                bursts.Clear();
                use_bursts = change.emission.burstCount != source.emission.burstCount;
                for (int i = 0; i < change.emission.burstCount; i++)
                {
                    if (source.emission.burstCount <= i)
                    {
                        use_bursts = true;
                        break;
                    }
                    PSBurst s = new PSBurst();
                    s.Parse(source.emission.GetBurst(i));
                    PSBurst c = new PSBurst();
                    c.Parse(change.emission.GetBurst(i));
                    if (!JsonUtility.ToJson(s).Equals(JsonUtility.ToJson(c)))
                    {
                        use_bursts = true;
                        break;
                    }
                }
                if(use_bursts)
                {
                    for (int i = 0; i < change.emission.burstCount; i++)
                    {
                        PSBurst c = new PSBurst();
                        c.Parse(change.emission.GetBurst(i));
                        bursts.Add(c);
                    }
                }
            }
            
            //render
#if UNITY_EDITOR
            {
                ParticleSystemRenderer sourcePSR = source.GetComponent<ParticleSystemRenderer>();
                ParticleSystemRenderer changePSR = change.GetComponent<ParticleSystemRenderer>();
                string sourceShareMatName = sourcePSR.sharedMaterial ? UnityEditor.AssetDatabase.GetAssetPath(sourcePSR.sharedMaterial) : "";
                string changeShareMatName = changePSR.sharedMaterial ? UnityEditor.AssetDatabase.GetAssetPath(changePSR.sharedMaterial) : "";
                if (!sourceShareMatName.Equals(changeShareMatName))
                {
                    use_shareMaterial = true;
                    shareMaterial = changeShareMatName.Replace("Assets/Export/Materials/","").Replace(".mat","");
                }
                string sourceTrailMatName = sourcePSR.trailMaterial ? UnityEditor.AssetDatabase.GetAssetPath(sourcePSR.trailMaterial) : "";
                string changeTrailMatName = changePSR.trailMaterial ? UnityEditor.AssetDatabase.GetAssetPath(changePSR.trailMaterial) : "";
                if (!sourceTrailMatName.Equals(changeTrailMatName))
                {
                    use_trailMaterial = true;
                    trailMaterial = changeTrailMatName.Replace("Assets/Export/Materials/","").Replace(".mat","");
                }
            }
#endif
        }
        
        public bool HasChange()
        {
            if (use_bursts || use_shareMaterial || use_trailMaterial) return true;
            return false;
        }

        public void Cover(ParticleSystem dest)
        {
            if(null == dest) return;
            
            if (use_bursts)
            {
                ParticleSystem.Burst[] temp = new ParticleSystem.Burst[bursts.Count];
                for (int i = 0; i < bursts.Count; i++)
                {
                    temp[i] = bursts[i].Coverto();
                }
                dest.emission.SetBursts(temp);
            }

            ParticleSystemRenderer renderer = dest.GetComponent<ParticleSystemRenderer>();
            if (use_shareMaterial)
            {
                if (string.IsNullOrEmpty(shareMaterial))
                {
                    renderer.sharedMaterial = null;
                }
                else
                {

                    if (!Application.isPlaying)
                    {
#if UNITY_EDITOR
                        renderer.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/Export/Materials/{shareMaterial}.mat");       
#endif
                    }
                    else
                    {
                        //renderer.sharedMaterial = DragonU3DSDK.Asset.ResourcesManager.Instance.LoadResource<Material>(
                        //    Path.Combine(PathManager.ROOM_MATERIAL_PATH.Substring(0, PathManager.ROOM_MATERIAL_PATH.LastIndexOf("/")), shareMaterial));
                    }
                }
            }
            if (use_trailMaterial)
            {
                if (string.IsNullOrEmpty(trailMaterial))
                {
                    renderer.trailMaterial = null;
                }
                else
                {              
                    if (!Application.isPlaying)
                    {
#if UNITY_EDITOR      
                        renderer.trailMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/Export/Materials/{trailMaterial}.mat");
#endif
                    }
                    else
                    {
                        //renderer.sharedMaterial = DragonU3DSDK.Asset.ResourcesManager.Instance.LoadResource<Material>(
                        //    Path.Combine(PathManager.ROOM_MATERIAL_PATH.Substring(0, PathManager.ROOM_MATERIAL_PATH.LastIndexOf("/")), trailMaterial));

                    }
                }
            }
        }
    }
    
    [Serializable]
    public class PSCommonVariant
    {
        public VariantMember<bool> useAutoRandomSeed = new VariantMember<bool>();
        public VariantMember<uint> randomSeed = new VariantMember<uint>();

        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            Debug.Log(ps.useAutoRandomSeed = ps.useAutoRandomSeed);
            Debug.Log(ps.randomSeed = ps.randomSeed);
        }
    }
    
    [Serializable]
    public class PSMainVariant
    {
        public VariantMember<float> duration = new VariantMember<float>();
        public VariantMember<bool> loop = new VariantMember<bool>();
        public VariantMember<bool> prewarm = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> startDelay = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startLifetime = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startSpeed = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<bool> startSize3D = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> startSize = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startSizeX = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startSizeY = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startSizeZ = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<bool> startRotation3D = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> startRotation = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startRotationX = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startRotationY = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> startRotationZ = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<float> flipRotation = new VariantMember<float>();
        public VariantMember<ParticleSystem.MinMaxGradient> startColor = new VariantMember<ParticleSystem.MinMaxGradient>();
        public VariantMember<ParticleSystem.MinMaxCurve> gravityModifier = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystemSimulationSpace> simulationSpace = new VariantMember<ParticleSystemSimulationSpace>();
        public VariantMember<float> simulationSpeed = new VariantMember<float>();
        public VariantMember<bool> useUnscaledTime = new VariantMember<bool>();
        public VariantMember<ParticleSystemScalingMode> scalingMode = new VariantMember<ParticleSystemScalingMode>();
        public VariantMember<bool> playOnAwake = new VariantMember<bool>();
        public VariantMember<ParticleSystemEmitterVelocityMode> emitterVelocityMode = new VariantMember<ParticleSystemEmitterVelocityMode>();
        public VariantMember<int> maxParticles = new VariantMember<int>();
        public VariantMember<ParticleSystemStopAction> stopAction = new VariantMember<ParticleSystemStopAction>();
        public VariantMember<ParticleSystemCullingMode> cullingMode = new VariantMember<ParticleSystemCullingMode>();
        public VariantMember<ParticleSystemRingBufferMode> ringBufferMode = new VariantMember<ParticleSystemRingBufferMode>();
        public VariantMember<Vector2> ringBufferLoopRange = new VariantMember<Vector2>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.MainModule main = ps.main;
            Debug.Log(main.duration = main.duration);
            Debug.Log(main.loop = main.loop);
            Debug.Log(main.prewarm = main.prewarm);
            Debug.Log(main.startDelay = main.startDelay);
            Debug.Log(main.startLifetime = main.startLifetime);
            Debug.Log(main.startSpeed = main.startSpeed);
            Debug.Log(main.startSize3D = main.startSize3D);
            Debug.Log(main.startSize = main.startSize);
            Debug.Log(main.startSizeX = main.startSizeX);
            Debug.Log(main.startSizeY = main.startSizeY);
            Debug.Log(main.startSizeZ = main.startSizeZ);
            Debug.Log(main.startRotation3D = main.startRotation3D);
            Debug.Log(main.startRotation = main.startRotation);
            Debug.Log(main.startRotationX = main.startRotationX);
            Debug.Log(main.startRotationY = main.startRotationY);
            Debug.Log(main.startRotationZ = main.startRotationZ);
            Debug.Log(main.flipRotation = main.flipRotation);
            Debug.Log(main.startColor = main.startColor);
            Debug.Log(main.gravityModifier = main.gravityModifier);
            Debug.Log(main.simulationSpace = main.simulationSpace);
            Debug.Log(main.simulationSpeed = main.simulationSpeed);
            Debug.Log(main.useUnscaledTime = main.useUnscaledTime);
            Debug.Log(main.scalingMode = main.scalingMode);
            Debug.Log(main.playOnAwake = main.playOnAwake);
            Debug.Log(main.emitterVelocityMode = main.emitterVelocityMode);
            Debug.Log(main.maxParticles = main.maxParticles);
            Debug.Log(main.stopAction = main.stopAction);
            Debug.Log(main.cullingMode = main.cullingMode);
            Debug.Log(main.ringBufferMode = main.ringBufferMode);
            Debug.Log(main.ringBufferLoopRange = main.ringBufferLoopRange);
        }
    }
    
    [Serializable]
    public class PSEmissionVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> rateOverTime = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> rateOverDistance = new VariantMember<ParticleSystem.MinMaxCurve>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.EmissionModule emission = ps.emission;
            Debug.Log(emission.enabled = emission.enabled);
            Debug.Log(emission.rateOverTime = emission.rateOverTime);
            Debug.Log(emission.rateOverDistance = emission.rateOverDistance);
        }
    }
    
    [Serializable]
    public class PSShapeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystemShapeType> shapeType = new VariantMember<ParticleSystemShapeType>();
        public VariantMember<float> angle = new VariantMember<float>();
        public VariantMember<float> radius = new VariantMember<float>();
        public VariantMember<float> donutRadius = new VariantMember<float>();
        public VariantMember<float> radiusThickness = new VariantMember<float>();
        public VariantMember<float> arc = new VariantMember<float>();
        public VariantMember<ParticleSystemShapeMultiModeValue> arcMode = new VariantMember<ParticleSystemShapeMultiModeValue>();
        public VariantMember<float> arcSpread = new VariantMember<float>();
        public VariantMember<ParticleSystem.MinMaxCurve> arcSpeed = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<float> length = new VariantMember<float>();
        //texture
        public VariantMember<Vector3> position = new VariantMember<Vector3>();
        public VariantMember<Vector3> rotation = new VariantMember<Vector3>();
        public VariantMember<Vector3> scale = new VariantMember<Vector3>();
        public VariantMember<bool> alignToDirection = new VariantMember<bool>();
        public VariantMember<float> randomDirectionAmount = new VariantMember<float>();
        public VariantMember<float> sphericalDirectionAmount = new VariantMember<float>();
        public VariantMember<float> randomPositionAmount = new VariantMember<float>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.ShapeModule shape = ps.shape;
            Debug.Log(shape.enabled = shape.enabled);
            Debug.Log(shape.shapeType = shape.shapeType);
            Debug.Log(shape.angle = shape.angle);
            Debug.Log(shape.radius = shape.radius);
            Debug.Log(shape.donutRadius = shape.donutRadius);
            Debug.Log(shape.radiusThickness = shape.radiusThickness);
            Debug.Log(shape.arc = shape.arc);
            Debug.Log(shape.arcMode = shape.arcMode);
            Debug.Log(shape.arcSpread = shape.arcSpread);
            Debug.Log(shape.arcSpeed = shape.arcSpeed);
            Debug.Log(shape.length = shape.length);
            Debug.Log(shape.position = shape.position);
            Debug.Log(shape.rotation = shape.rotation);
            Debug.Log(shape.scale = shape.scale);
            Debug.Log(shape.alignToDirection = shape.alignToDirection);
            Debug.Log(shape.randomDirectionAmount = shape.randomDirectionAmount);
            Debug.Log(shape.sphericalDirectionAmount = shape.sphericalDirectionAmount);
            Debug.Log(shape.randomPositionAmount = shape.randomPositionAmount);
        }
    }
    
    [Serializable]
    public class PSVelocityOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> x = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> y = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> z = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystemSimulationSpace> space = new VariantMember<ParticleSystemSimulationSpace>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalX = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalY = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalZ = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalOffsetX = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalOffsetY = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> orbitalOffsetZ = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> radial = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> speedModifier = new VariantMember<ParticleSystem.MinMaxCurve>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
            Debug.Log(velocityOverLifetime.enabled = velocityOverLifetime.enabled);
            Debug.Log(velocityOverLifetime.x = velocityOverLifetime.x);
            Debug.Log(velocityOverLifetime.y = velocityOverLifetime.y);
            Debug.Log(velocityOverLifetime.z = velocityOverLifetime.z);
            Debug.Log(velocityOverLifetime.space = velocityOverLifetime.space);
            Debug.Log(velocityOverLifetime.orbitalX = velocityOverLifetime.orbitalX);
            Debug.Log(velocityOverLifetime.orbitalY = velocityOverLifetime.orbitalY);
            Debug.Log(velocityOverLifetime.orbitalZ = velocityOverLifetime.orbitalZ);
            Debug.Log(velocityOverLifetime.orbitalOffsetX = velocityOverLifetime.orbitalOffsetX);
            Debug.Log(velocityOverLifetime.orbitalOffsetY = velocityOverLifetime.orbitalOffsetY);
            Debug.Log(velocityOverLifetime.orbitalOffsetZ = velocityOverLifetime.orbitalOffsetZ);
            Debug.Log(velocityOverLifetime.radial = velocityOverLifetime.radial);
            Debug.Log(velocityOverLifetime.speedModifier = velocityOverLifetime.speedModifier);
        }
    }
    
    [Serializable]
    public class PSLimitVelocityOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<bool> separateAxes = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> limit = new VariantMember<ParticleSystem.MinMaxCurve>();    
        public VariantMember<ParticleSystem.MinMaxCurve> limitX = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> limitY = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> limitZ = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystemSimulationSpace> space = new VariantMember<ParticleSystemSimulationSpace>();
        public VariantMember<float> dampen = new VariantMember<float>();
        public VariantMember<ParticleSystem.MinMaxCurve> drag = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<bool> multiplyDragByParticleSize = new VariantMember<bool>();
        public VariantMember<bool> multiplyDragByParticleVelocity = new VariantMember<bool>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
            Debug.Log(limitVelocityOverLifetime.enabled = limitVelocityOverLifetime.enabled);
            Debug.Log(limitVelocityOverLifetime.separateAxes = limitVelocityOverLifetime.separateAxes);
            Debug.Log(limitVelocityOverLifetime.limit = limitVelocityOverLifetime.limit);
            Debug.Log(limitVelocityOverLifetime.limitX = limitVelocityOverLifetime.limitX);
            Debug.Log(limitVelocityOverLifetime.limitY = limitVelocityOverLifetime.limitY);
            Debug.Log(limitVelocityOverLifetime.limitZ = limitVelocityOverLifetime.limitZ);
            Debug.Log(limitVelocityOverLifetime.space = limitVelocityOverLifetime.space);
            Debug.Log(limitVelocityOverLifetime.dampen = limitVelocityOverLifetime.dampen);
            Debug.Log(limitVelocityOverLifetime.drag = limitVelocityOverLifetime.drag);
            Debug.Log(limitVelocityOverLifetime.multiplyDragByParticleSize = limitVelocityOverLifetime.multiplyDragByParticleSize);
            Debug.Log(limitVelocityOverLifetime.multiplyDragByParticleVelocity = limitVelocityOverLifetime.multiplyDragByParticleVelocity);
        }
    }
    
    [Serializable]
    public class PSForceOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> x = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> y = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> z = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystemSimulationSpace> space = new VariantMember<ParticleSystemSimulationSpace>();
        public VariantMember<bool> randomized = new VariantMember<bool>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.ForceOverLifetimeModule forceOverLifetime = ps.forceOverLifetime;
            Debug.Log(forceOverLifetime.enabled = forceOverLifetime.enabled);
            Debug.Log(forceOverLifetime.x = forceOverLifetime.x);
            Debug.Log(forceOverLifetime.y = forceOverLifetime.y);
            Debug.Log(forceOverLifetime.z = forceOverLifetime.z);
            Debug.Log(forceOverLifetime.space = forceOverLifetime.space);
            Debug.Log(forceOverLifetime.randomized = forceOverLifetime.randomized);
        }
    }
    
    [Serializable]
    public class PSColorOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxGradient> color = new VariantMember<ParticleSystem.MinMaxGradient>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            Debug.Log(colorOverLifetime.enabled = colorOverLifetime.enabled);
            Debug.Log(colorOverLifetime.color = colorOverLifetime.color);
        }
    }
    
    [Serializable]
    public class PSSizeOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<bool> separateAxes = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> size = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> x = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> y = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> z = new VariantMember<ParticleSystem.MinMaxCurve>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            Debug.Log(sizeOverLifetime.enabled = sizeOverLifetime.enabled);
            Debug.Log(sizeOverLifetime.separateAxes = sizeOverLifetime.separateAxes);
            Debug.Log(sizeOverLifetime.size = sizeOverLifetime.size);
            Debug.Log(sizeOverLifetime.x = sizeOverLifetime.x);
            Debug.Log(sizeOverLifetime.y = sizeOverLifetime.y);
            Debug.Log(sizeOverLifetime.z = sizeOverLifetime.z);
        }
    }
    
    [Serializable]
    public class PSSizeBySpeedVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<bool> separateAxes = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> size = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> x = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> y = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> z = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<Vector2> range = new VariantMember<Vector2>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.SizeBySpeedModule sizeBySpeed = ps.sizeBySpeed;
            Debug.Log(sizeBySpeed.enabled = sizeBySpeed.enabled);
            Debug.Log(sizeBySpeed.separateAxes = sizeBySpeed.separateAxes);
            Debug.Log(sizeBySpeed.size = sizeBySpeed.size);
            Debug.Log(sizeBySpeed.x = sizeBySpeed.x);
            Debug.Log(sizeBySpeed.y = sizeBySpeed.y);
            Debug.Log(sizeBySpeed.z = sizeBySpeed.z);
            Debug.Log(sizeBySpeed.range = sizeBySpeed.range);
        }
    }
    
    [Serializable]
    public class PSRotationOverLifetimeVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<bool> separateAxes = new VariantMember<bool>();
        public VariantMember<ParticleSystem.MinMaxCurve> x = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> y = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<ParticleSystem.MinMaxCurve> z = new VariantMember<ParticleSystem.MinMaxCurve>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = ps.rotationOverLifetime;
            Debug.Log(rotationOverLifetime.enabled = rotationOverLifetime.enabled);
            Debug.Log(rotationOverLifetime.separateAxes = rotationOverLifetime.separateAxes);
            Debug.Log(rotationOverLifetime.x = rotationOverLifetime.x);
            Debug.Log(rotationOverLifetime.y = rotationOverLifetime.y);
            Debug.Log(rotationOverLifetime.z = rotationOverLifetime.z);
        }
    }
    
    [Serializable]
    public class PSTextureSheetAnimationVariant
    {
        public VariantMember<bool> enabled = new VariantMember<bool>();
        public VariantMember<ParticleSystemAnimationMode> mode = new VariantMember<ParticleSystemAnimationMode>();
        public VariantMember<int> numTilesX = new VariantMember<int>();
        public VariantMember<int> numTilesY = new VariantMember<int>();
        public VariantMember<ParticleSystemAnimationType> animation = new VariantMember<ParticleSystemAnimationType>();
        public VariantMember<ParticleSystemAnimationRowMode> rowMode = new VariantMember<ParticleSystemAnimationRowMode>();
        public VariantMember<ParticleSystemAnimationTimeMode> timeMode = new VariantMember<ParticleSystemAnimationTimeMode>();
        public VariantMember<Vector2> speedRange = new VariantMember<Vector2>();
        public VariantMember<ParticleSystem.MinMaxCurve> frameOverTime = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<float> fps = new VariantMember<float>();
        public VariantMember<ParticleSystem.MinMaxCurve> startFrame = new VariantMember<ParticleSystem.MinMaxCurve>();
        public VariantMember<int> cycleCount = new VariantMember<int>();
        public VariantMember<UVChannelFlags> uvChannelMask = new VariantMember<UVChannelFlags>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystem ps = new ParticleSystem();
            ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = ps.textureSheetAnimation;
            Debug.Log(textureSheetAnimation.enabled = textureSheetAnimation.enabled);
            Debug.Log(textureSheetAnimation.mode = textureSheetAnimation.mode);
            Debug.Log(textureSheetAnimation.numTilesX = textureSheetAnimation.numTilesX);
            Debug.Log(textureSheetAnimation.numTilesY = textureSheetAnimation.numTilesY);
            Debug.Log(textureSheetAnimation.animation = textureSheetAnimation.animation);
            Debug.Log(textureSheetAnimation.rowMode = textureSheetAnimation.rowMode);
            Debug.Log(textureSheetAnimation.timeMode = textureSheetAnimation.timeMode);
            Debug.Log(textureSheetAnimation.speedRange = textureSheetAnimation.speedRange);
            Debug.Log(textureSheetAnimation.frameOverTime = textureSheetAnimation.frameOverTime);
            Debug.Log(textureSheetAnimation.fps = textureSheetAnimation.fps);
            Debug.Log(textureSheetAnimation.startFrame = textureSheetAnimation.startFrame);
            Debug.Log(textureSheetAnimation.cycleCount = textureSheetAnimation.cycleCount);
            Debug.Log(textureSheetAnimation.uvChannelMask = textureSheetAnimation.uvChannelMask);
        }
    }
    
    [Serializable]
    public class PSRendererVariant
    {
        public VariantMember<ParticleSystemRenderMode> renderMode = new VariantMember<ParticleSystemRenderMode>();
        public VariantMember<float> cameraVelocityScale = new VariantMember<float>();
        public VariantMember<float> velocityScale = new VariantMember<float>();
        public VariantMember<float> lengthScale = new VariantMember<float>();
        public VariantMember<float> normalDirection = new VariantMember<float>();
        public VariantMember<ParticleSystemSortMode> sortMode = new VariantMember<ParticleSystemSortMode>();
        public VariantMember<float> sortingFudge = new VariantMember<float>();
        public VariantMember<float> minParticleSize = new VariantMember<float>();
        public VariantMember<float> maxParticleSize = new VariantMember<float>();
        public VariantMember<ParticleSystemRenderSpace> alignment = new VariantMember<ParticleSystemRenderSpace>();
        public VariantMember<Vector3> flip = new VariantMember<Vector3>();
        public VariantMember<bool> allowRoll = new VariantMember<bool>();
        public VariantMember<Vector3> pivot = new VariantMember<Vector3>();
        //Visualize Pivot
        public VariantMember<uint> renderingLayerMask = new VariantMember<uint>();
        //Apply Active Color Space
        //Custom Vertex Streams
        public VariantMember<ShadowCastingMode> shadowCastingMode = new VariantMember<ShadowCastingMode>();
        public VariantMember<bool> receiveShadows = new VariantMember<bool>();
        public VariantMember<float> shadowBias = new VariantMember<float>();
        public VariantMember<MotionVectorGenerationMode> motionVectorGenerationMode = new VariantMember<MotionVectorGenerationMode>();
        public VariantMember<int> sortingLayerID = new VariantMember<int>();
        public VariantMember<int> sortingOrder = new VariantMember<int>();
        public VariantMember<LightProbeUsage> lightProbeUsage = new VariantMember<LightProbeUsage>();
        public VariantMember<ReflectionProbeUsage> reflectionProbeUsage = new VariantMember<ReflectionProbeUsage>();
        
        [Preserve]
        public void PreserveCodeStrip()
        {
            ParticleSystemRenderer renderer = new ParticleSystemRenderer();
            Debug.Log(renderer.renderMode = renderer.renderMode);
            Debug.Log(renderer.cameraVelocityScale = renderer.cameraVelocityScale);
            Debug.Log(renderer.velocityScale = renderer.velocityScale);
            Debug.Log(renderer.lengthScale = renderer.lengthScale);
            Debug.Log(renderer.normalDirection = renderer.normalDirection);
            Debug.Log(renderer.sortMode = renderer.sortMode);
            Debug.Log(renderer.sortingFudge = renderer.sortingFudge);
            Debug.Log(renderer.minParticleSize = renderer.minParticleSize);
            Debug.Log(renderer.maxParticleSize = renderer.maxParticleSize);
            Debug.Log(renderer.alignment = renderer.alignment);
            Debug.Log(renderer.flip = renderer.flip);
            Debug.Log(renderer.allowRoll = renderer.allowRoll);
            Debug.Log(renderer.pivot = renderer.pivot);
            Debug.Log(renderer.renderingLayerMask = renderer.renderingLayerMask);
            Debug.Log(renderer.shadowCastingMode = renderer.shadowCastingMode);
            Debug.Log(renderer.receiveShadows = renderer.receiveShadows);
            Debug.Log(renderer.shadowBias = renderer.shadowBias);
            Debug.Log(renderer.motionVectorGenerationMode = renderer.motionVectorGenerationMode);
            Debug.Log(renderer.sortingLayerID = renderer.sortingLayerID);
            Debug.Log(renderer.sortingOrder = renderer.sortingOrder);
            Debug.Log(renderer.lightProbeUsage = renderer.lightProbeUsage);
            Debug.Log(renderer.reflectionProbeUsage = renderer.reflectionProbeUsage);
        }
    }

    public class ParticleSystemVariantUtils
    {
        public static string Serialize(ParticleSystem source, ParticleSystem change)
        {
            JObject jObject = new JObject();
            
            PSSpecialVariant special = new PSSpecialVariant();
            special.Variants(source, change);
            if(special.HasChange())
            {
                jObject[special.GetType().ToString()] = JsonUtility.ToJson(special);
            }

            PSCommonVariant common = new PSCommonVariant();
            VariantUtils.Variants(common, source, change);
            {
                if (change.useAutoRandomSeed)
                {
                    common.randomSeed.use = false;
                }
            }
            if (VariantUtils.HasChange(common))
            {
                JObject subObj = new JObject();
                jObject[common.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, common);
            }
            
            PSMainVariant main = new PSMainVariant();
            VariantUtils.Variants(main, source.main, change.main);
            if (VariantUtils.HasChange(main))
            {
                JObject subObj = new JObject();
                jObject[main.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, main);
            }
            PSEmissionVariant emission = new PSEmissionVariant();
            VariantUtils.Variants(emission, source.emission, change.emission);
            if (VariantUtils.HasChange(emission))
            {
                JObject subObj = new JObject();
                jObject[emission.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, emission);
            }
            PSShapeVariant shape = new PSShapeVariant();
            VariantUtils.Variants(shape, source.shape, change.shape);
            if (VariantUtils.HasChange(shape))
            {
                JObject subObj = new JObject();
                jObject[shape.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, shape);
            }
            PSVelocityOverLifetimeVariant velocityOverLifetime = new PSVelocityOverLifetimeVariant();
            VariantUtils.Variants(velocityOverLifetime, source.velocityOverLifetime, change.velocityOverLifetime);
            if (VariantUtils.HasChange(velocityOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[velocityOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, velocityOverLifetime);
            }
            PSLimitVelocityOverLifetimeVariant limitVelocityOverLifetime = new PSLimitVelocityOverLifetimeVariant();
            VariantUtils.Variants(limitVelocityOverLifetime, source.limitVelocityOverLifetime, change.limitVelocityOverLifetime);
            if (VariantUtils.HasChange(limitVelocityOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[limitVelocityOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, limitVelocityOverLifetime);
            }
            PSForceOverLifetimeVariant forceOverLifetime = new PSForceOverLifetimeVariant();
            VariantUtils.Variants(forceOverLifetime, source.forceOverLifetime, change.forceOverLifetime);
            if (VariantUtils.HasChange(forceOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[forceOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, forceOverLifetime);
            }
            PSColorOverLifetimeVariant colorOverLifetime = new PSColorOverLifetimeVariant();
            VariantUtils.Variants(colorOverLifetime, source.colorOverLifetime, change.colorOverLifetime);
            if (VariantUtils.HasChange(colorOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[colorOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, colorOverLifetime);
            }
            PSSizeOverLifetimeVariant sizeOverLifetime = new PSSizeOverLifetimeVariant();
            VariantUtils.Variants(sizeOverLifetime, source.sizeOverLifetime, change.sizeOverLifetime);
            if (VariantUtils.HasChange(sizeOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[sizeOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, sizeOverLifetime);
            }
            PSSizeBySpeedVariant sizeBySpeed = new PSSizeBySpeedVariant();
            VariantUtils.Variants(sizeBySpeed, source.sizeBySpeed, change.sizeBySpeed);
            if (VariantUtils.HasChange(sizeBySpeed))
            {
                JObject subObj = new JObject();
                jObject[sizeBySpeed.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, sizeBySpeed);
            }
            PSRotationOverLifetimeVariant rotationOverLifetime = new PSRotationOverLifetimeVariant();
            VariantUtils.Variants(rotationOverLifetime, source.rotationOverLifetime, change.rotationOverLifetime);
            if (VariantUtils.HasChange(rotationOverLifetime))
            {
                JObject subObj = new JObject();
                jObject[rotationOverLifetime.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, rotationOverLifetime);
            }
            PSTextureSheetAnimationVariant textureSheetAnimation = new PSTextureSheetAnimationVariant();
            VariantUtils.Variants(textureSheetAnimation, source.textureSheetAnimation, change.textureSheetAnimation);
            if (VariantUtils.HasChange(textureSheetAnimation))
            {
                JObject subObj = new JObject();
                jObject[textureSheetAnimation.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, textureSheetAnimation);
            }
            PSRendererVariant renderer = new PSRendererVariant();
            VariantUtils.Variants(renderer, source.GetComponent<ParticleSystemRenderer>(), change.GetComponent<ParticleSystemRenderer>());
            if (VariantUtils.HasChange(renderer))
            {
                JObject subObj = new JObject();
                jObject[renderer.GetType().ToString()] = subObj;
                VariantUtils.Serialize(subObj, renderer);
            }

            return jObject.ToString();
        }

        public static void Deserialize(ParticleSystem dest, JObject json)
        {
            foreach (JProperty p in json.Properties())
            {
                if (p.Name.Equals(typeof(PSSpecialVariant).ToString()))
                {
                    PSSpecialVariant variant = JsonUtility.FromJson<PSSpecialVariant>(p.Value.ToString());
                    variant.Cover(dest);
                }
                else if (p.Name.Equals(typeof(PSCommonVariant).ToString()))
                {
                    PSCommonVariant variant = new PSCommonVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest);
                }
                else if (p.Name.Equals(typeof(PSMainVariant).ToString()))
                {
                    PSMainVariant variant = new PSMainVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.main);
                }
                else if (p.Name.Equals(typeof(PSEmissionVariant).ToString()))
                {
                    PSEmissionVariant variant = new PSEmissionVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.emission);
                }
                else if (p.Name.Equals(typeof(PSShapeVariant).ToString()))
                {
                    PSShapeVariant variant = new PSShapeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.shape);
                }
                else if (p.Name.Equals(typeof(PSVelocityOverLifetimeVariant).ToString()))
                {
                    PSVelocityOverLifetimeVariant variant = new PSVelocityOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.velocityOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSLimitVelocityOverLifetimeVariant).ToString()))
                {
                    PSLimitVelocityOverLifetimeVariant variant = new PSLimitVelocityOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.limitVelocityOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSForceOverLifetimeVariant).ToString()))
                {
                    PSForceOverLifetimeVariant variant = new PSForceOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.forceOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSColorOverLifetimeVariant).ToString()))
                {
                    PSColorOverLifetimeVariant variant = new PSColorOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.colorOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSSizeOverLifetimeVariant).ToString()))
                {
                    PSSizeOverLifetimeVariant variant = new PSSizeOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.sizeOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSSizeBySpeedVariant).ToString()))
                {
                    PSSizeBySpeedVariant variant = new PSSizeBySpeedVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.sizeBySpeed);
                }
                else if (p.Name.Equals(typeof(PSRotationOverLifetimeVariant).ToString()))
                {
                    PSRotationOverLifetimeVariant variant = new PSRotationOverLifetimeVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.rotationOverLifetime);
                }
                else if (p.Name.Equals(typeof(PSTextureSheetAnimationVariant).ToString()))
                {
                    PSTextureSheetAnimationVariant variant = new PSTextureSheetAnimationVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.textureSheetAnimation);
                }
                else if (p.Name.Equals(typeof(PSRendererVariant).ToString()))
                {
                    PSRendererVariant variant = new PSRendererVariant();
                    VariantUtils.Deserialize(variant, (JObject)p.Value);
                    VariantUtils.Cover(variant, dest.GetComponent<ParticleSystemRenderer>());
                }
            }
        }
    }

    [Serializable]
    public class ParticleSystemVariant
    {
        public string path;
        public string json;
    }
    
    //Transform 变体方案：全量
    [Serializable]
    public class TransformVariant
    {
        public string path;
        public bool active;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
    }
}