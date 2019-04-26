//#using #System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System;
namespace PlantSystem {
    public static class StaticRandom
    {
        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
        public static int Next(int val) { return random.Value.Next(val); }
        public static int Next() { return random.Value.Next(); }
    }
    public class Gene {
        //private static readonly System.Random RANDOM = new System.Random();
        const string SYMBOLS = "xXyYzZ[]STFLR1234567890\n\n\n";
        /* ORIENTATION SYMBOLS
         * xX: yaw 
         * yY: pitch
         * zZ: roll
        /* CONTROL SYMBOLS
         * []: branch(save/restore state)
         * \n: seperate rule
        /* MASS SYMBOLS
         * S: Seed
         * T: sTem
        /* MASS TYPES SYMBOL(It will overwrite all symbols followed)
         * F: Flower
         * L: Leaf
         * R: fRuit
        /* PLACEHOLDER SYMBOLS
         * Numbers: place holders
         */
        public string rules{get; set;} = null;
        public int[] parameters{get; set;} = new int[5];
        /* 0: random seed
         * 1: default yaw
         * 2: default pitch
         * 3: default roll
         * 4: default length
         */
        public Gene() { }
        public Gene(string _rules, int[] _parameters) {
            rules = _rules;
            for(int i=0; i<parameters.Length; ++i)
                parameters[i] = _parameters[i];
        }
        public Gene Random(int length = 100) {
            /* Default Rules:
             * S -> TTTT[-L][+L]
             * L -> L
             * T -> T
             * F -> F
             * ? -> ?
             */
            var symbols = new char[length];
            for(int i=0; i<length; ++i)
                symbols[i] = SYMBOLS[StaticRandom.Next(SYMBOLS.Length)];
            rules = new string(symbols);
            for(int i=0; i<parameters.Length; ++i)
                parameters[i] = StaticRandom.Next();
            return this;
        }
        private static string mutate(string rules, ){
            return rules;
        }
        public Gene florification(Gene partner) {
            Gene daughter = new Gene();
            for(int i=0; i<daughter.parameters.Length; ++i)
                if(StaticRandom.Next(2) == 1) 
                    daughter.parameters[i] = parameters[i];
                else 
                    daughter.parameters[i] = partner.parameters[i];
            int i,j;
            if(rules.Count < partners.rules.Count){
                j = StaticRandom.Next(partners.rules.Count);
                i = rules.Count*j/partners.rules.Count;
            } else{
                i = StaticRandom.Next(rules.Count);
                j = partners.rules.Count*i/rules.Count;
            }
            rules.Substring(0,i) + partners.rules.Substring(j)
        }
        public override string ToString() {
            return rules;
        }
    }

    public class Rule {
        Dictionary<char, List<string>> rules = new Dictionary<char, List<string>>();
        public Rule(){
        }
        public Rule(Gene gene){
            FromGene(gene);
        }
        public Rule FromGene(Gene gene){
            foreach(string rule in gene.rules.Split('\n')){
                if(rule.Length <= 1) continue; 
                if(rules.ContainsKey(rule[0])) rules[rule[0]].Add(rule);
                else rules[rule[0]] = new List<string>{rule};
            }
            return this;
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach(var pair in rules)
                foreach(var rule in pair.Value)
                sb.Append($"{pair.Key} -> {rule}\n");
            return sb.ToString();
        }
        public List<string> this[char target] {
            get {
                List<string> rule = null;
                rules.TryGetValue(target, out rule);
                return rule;
            }
        }
    }

    public class Plant {
        /* RESTRICITON OF RULE FOR PLANT REPRELENTATION
         * Any Rule: P -> PS
         * (succesor must have the predecessor at the start)
         * State: T[TT]T ==(Rule)==> T[TT']T'
         * (only last symbol of the branch produce next step)
         * State: {MASS_SYMBOL} => {ALL SYMBOLS  
         */
        protected string state = "S";
        protected string mass_type = "S";
        protected Queue<int> developing_indexes {get;} = new Queue<int>(new[]{0});
        protected Gene gene; 
        protected Rule rule;
        public Plant(){ }
        public Plant FromGene(Gene _gene){
            state = "S";
            rule = new Rule(_gene);
            FixRule();
            return this;
        }
        public Plant Step(){
            StringBuilder sb = new StringBuilder();
            int last_developing_index = 0;
            Random random = new Random(gene.parameters[0]);
            for(int _i=developing_indexes.Count; _i>0; --_i){
                int developing_index = developing_indexes.Dequeue();
                char predecessor = state[developing_index];
                List<string> succesors = rule[predecessor];
                if(succesors == null) continue;
                string succesor = succesors[random.Next(succesors.Count)];
                int index = 0;
                while(-1 != (index = succesor.IndexOf(']', index+1)))
                    developing_indexes.Enqueue(developing_index + index - 1);
                developing_indexes.Enqueue(developing_index + succesor.Length - 1);
                sb.Append(state.Substring(last_developing_index, developing_index-last_developing_index));
                sb.Append(succesor);
                last_developing_index = developing_index + 1;
            }
            if(last_developing_index < state.Length) sb.Append(state.Substring(last_developing_index));
            state = sb.ToString();
            return this;
        }
        public override string ToString() {
            return state;
        }
    }

#if DEBUG
    public class Test {
        static void Main(string[] args){
            Gene gene = new Gene().Random(100);
            System.Console.WriteLine($"random gene:\n {gene.ToString()}\n");
            Rule rule = new Rule().FromGene(gene);
            System.Console.WriteLine($"rule from gene:\n{rule.ToString()}\n");
            Plant plant = new Plant().FromGene(gene);
            System.Console.WriteLine($"plant from gene:\n{plant.ToString()}\n");
            plant.Step();
            System.Console.WriteLine($"plant after 1 step:\n{plant.ToString()}\n");
            plant.Step();
            System.Console.WriteLine($"plant after 2 step:\n{plant.ToString()}\n");
        }
    }
#endif
}
